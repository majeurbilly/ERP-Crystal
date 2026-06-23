{
  description = "Crystal ERP — environnement de développement reproductible";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs = { self, nixpkgs, flake-utils }:
    flake-utils.lib.eachDefaultSystem (system:
      let
        pkgs = import nixpkgs {
          inherit system;
          config.allowUnfree = true;
        };

        dotnetSdk = pkgs.dotnet-sdk_9;
        root = self;

        # Scripts exécutables exposés comme `nix run .#<name>`
        mkApp = name: script: {
          type = "app";
          program = "${pkgs.writeShellScript name script}";
        };

        # Attend que frontend (3000) et backend (8080) répondent
        waitForStackScript = ''
          wait_for_url() {
            local url="$1"
            local label="$2"
            local i
            for i in $(seq 1 60); do
              if curl -sf "$url" -o /dev/null 2>/dev/null; then
                echo "  OK — $label ($url)"
                return 0
              fi
              sleep 2
            done
            echo "  Échec — $label inaccessible : $url"
            return 1
          }

          echo "Attente de la stack Docker..."
          wait_for_url "http://localhost:3000" "frontend" || exit 1
          wait_for_url "http://localhost:8080/swagger/index.html" "backend (swagger)" || exit 1
        '';

        dockerCheckScript = ''
          if ! docker info >/dev/null 2>&1; then
            echo "Erreur : le démon Docker n'est pas accessible."
            echo "Lancez Docker Desktop et activez l'intégration WSL si besoin."
            exit 1
          fi
        '';
      in {
        devShells.default = pkgs.mkShell {
          packages = with pkgs; [
            dotnetSdk
            dotnet-ef
            nodejs_22
            pnpm_10
            docker
            docker-compose
            git
            postgresql
            curl
            cacert
          ];

          shellHook = ''
            export DOTNET_ROOT="${dotnetSdk}"
            export PATH="$DOTNET_ROOT/bin:$PATH"
            export ASPNETCORE_ENVIRONMENT=Development
            export SSL_CERT_FILE="${pkgs.cacert}/etc/ssl/certs/ca-bundle.crt"

            # Docker Desktop (WSL) : activer l'intégration WSL pour cette distro
            if [ ! -S /var/run/docker.sock ] && [ -S "$HOME/.docker/run/docker.sock" ]; then
              export DOCKER_HOST="unix://$HOME/.docker/run/docker.sock"
            fi

            echo "Crystal ERP — shell Nix"
            echo "  dotnet:  $(dotnet --version)"
            echo "  node:    $(node --version)"
            echo "  pnpm:    $(pnpm --version)"
            if docker info >/dev/null 2>&1; then
              echo "  docker:  OK ($(docker version --format '{{.Server.Version}}' 2>/dev/null || echo connecté))"
            else
              echo "  docker:  indisponible (démon non démarré)"
              echo ""
              echo "  WSL + Docker Desktop :"
              echo "    1. Lancer Docker Desktop sur Windows"
              echo "    2. Settings → Resources → WSL Integration → activer cette distro"
              echo "    3. Relancer : nix develop"
            fi
            echo ""
            echo "Commandes utiles (nix run .#<name>) :"
            echo "  docker-up        — Docker Compose (build + détaché)"
            echo "  docker-up-wait   — idem + attente frontend/API"
            echo "  docker-down      — arrêter les conteneurs"
            echo "  backend-test     — dotnet test (solution complète)"
            echo "  frontend-test    — Vitest (frontend)"
            echo "  test-all         — backend + frontend"
            echo "  verify           — docker-up-wait + backend + frontend"
          '';
        };

        apps = {
          docker-up = mkApp "docker-up" ''
            set -euo pipefail
            cd "${root}"
            ${dockerCheckScript}
            docker compose -f docker-compose.yaml up -d --build
          '';

          docker-up-wait = mkApp "docker-up-wait" ''
            set -euo pipefail
            cd "${root}"
            ${dockerCheckScript}
            docker compose -f docker-compose.yaml up -d --build
            ${waitForStackScript}
          '';

          docker-down = mkApp "docker-down" ''
            set -euo pipefail
            cd "${root}"
            docker compose -f docker-compose.yaml down
          '';

          backend-build = mkApp "backend-build" ''
            set -euo pipefail
            cd "${root}/backend"
            dotnet build Crystal.sln
          '';

          backend-test = mkApp "backend-test" ''
            set -euo pipefail
            cd "${root}/backend"
            dotnet test Crystal.sln
          '';

          frontend-install = mkApp "frontend-install" ''
            set -euo pipefail
            cd "${root}/frontend"
            pnpm install --frozen-lockfile
          '';

          frontend-dev = mkApp "frontend-dev" ''
            set -euo pipefail
            cd "${root}/frontend"
            pnpm install --frozen-lockfile
            pnpm dev
          '';

          frontend-build = mkApp "frontend-build" ''
            set -euo pipefail
            cd "${root}/frontend"
            pnpm install --frozen-lockfile
            pnpm run build
          '';

          frontend-test = mkApp "frontend-test" ''
            set -euo pipefail
            cd "${root}/frontend"
            pnpm install --frozen-lockfile
            pnpm test --run
          '';

          test-all = mkApp "test-all" ''
            set -euo pipefail
            cd "${root}/backend"
            dotnet test Crystal.sln
            cd "${root}/frontend"
            pnpm install --frozen-lockfile
            pnpm test --run
          '';

          verify = mkApp "verify" ''
            set -euo pipefail
            cd "${root}"
            ${dockerCheckScript}
            docker compose -f docker-compose.yaml up -d --build
            ${waitForStackScript}
            echo ""
            echo "=== Tests backend ==="
            cd "${root}/backend"
            dotnet test Crystal.sln
            echo ""
            echo "=== Tests frontend (Vitest) ==="
            cd "${root}/frontend"
            pnpm install --frozen-lockfile
            pnpm test --run
            echo ""
            echo "=== Vérification terminée ==="
          '';
        };

        formatter = pkgs.nixpkgs-fmt;
      });
}
