-- Script de secours : rétrocompatibilité des permissions inventaire
-- À exécuter manuellement sur une base de développement existante si les gérants
-- sont bloqués après l'introduction de LocationScope (valeur null refusée pour les écritures).
--
-- Contexte : avant la refonte, les lignes RolePermissions pour inventory_quantity
-- n'avaient pas de LocationScope. Le moteur exige désormais un scope explicite.

-- 1. Appliquer "all" à toutes les permissions inventaire legacy sans scope
UPDATE "RolePermissions"
SET "LocationScope" = 'all'
WHERE "Subject" = 'inventory_quantity'
  AND ("LocationScope" IS NULL OR TRIM("LocationScope") = '');

-- 2. Nettoyer d'éventuelles liaisons orphelines devenues incohérentes (scope global)
DELETE FROM "RolePermissionLocations"
WHERE "RolePermissionId" IN (
    SELECT "Id"
    FROM "RolePermissions"
    WHERE "Subject" = 'inventory_quantity'
      AND "LocationScope" = 'all'
);

-- 3. Vérification (lecture seule)
-- SELECT "DynamicRoleId", "Action", "Subject", "LocationScope"
-- FROM "RolePermissions"
-- WHERE "Subject" = 'inventory_quantity'
-- ORDER BY "DynamicRoleId", "Action";
