# 💎 ERP Crystal — Guide de Référence Frontend
Quand on se connecte à a notre ERP, la page ne se recharge jamais au complet. On utilise une "coquille" fixe, et seul le contenu au centre change.

Voici à quoi va ressembler l'écran de tes employés :
```
======================================================================
| 💎 ERP Crystal | 🏢 Succursale : Duberger | 👤 Bonjour, Billy (Admin) | [Déconnexion] |
======================================================================
| 📌 MENU PRINCIPAL |                                                |
|                   |  [ WIDGET 1 ]    [ WIDGET 2 ]    [ WIDGET 3 ]  |
| 📊 Tableau de bord|  Alerte Stock    Transferts      Heures sem.   |
|                   |                                                |
| 📦 INVENTAIRE     |------------------------------------------------|
|   - Livres        |                                                |
|   - Produits      |  Tableau complet, formulaire, ou graphique     |
|                   |  qui s'affiche ici selon le bouton cliqué      |
| 🚚 OPÉRATIONS     |  dans le menu de gauche.                       |
|   - Transferts    |                                                |
|   - Réceptions    |                                                |
|                   |                                                |
| 👥 R.H.           |                                                |
|   - Employés      |                                                |
|   - Horaires      |                                                |
======================================================================
```

- **Header :** L'identité, la succursale actuelle (très important pour les transferts), le nom de l'utilisateur et le bouton pour quitter.
    
- **À gauche (Sidebar) :** La navigation. **C'est ici que la magie des rôles opère.** Un simple employé ne verra pas le menu "👥 R.H." par exemple, le s'adapte à la personne connectée.
    
- **Au centre (Main Content) :** La zone de travail dynamique.
    

---

### 🗺️ 2. L'Arborescence des Pages (Le Plan de match)

Voici exactement comment les pages s'emboîtent dans notre application.

**La Zone Publique (Le Mur)**

- `/login` : La seule page visible de l'extérieur. Un fond propre, le logo, Courriel / User + Mot de passe. C'est tout.
    

**La Zone Privée (À l'intérieur de la coquille)**

- `/dashboard` : Page apres la page de login selon le statue du User
    
- `/inventaire/livres` : **La page complète.** Une grosse grille de données avec tous les livres. Des boutons pour "Ajouter", "Modifier", "Retirer".
    
- `/inventaire/produits` : **La page complète.** Même chose, mais adaptée aux produits physiques de la librairie.
    
- `/operations/transferts` : L'interface pour envoyer ou recevoir une boîte de livres entre ta succursale et une autre.
    
- `/operations/receptions` : L'interface pour scanner ou entrer la nouvelle marchandise qui arrive du fournisseur.
    
- `/rh/employes` : La liste du staff (Admin/Gérant seulement).
    
- `/rh/mon-profil` : Fiche de paie, horaire personnel et changement de mot de passe.
    

---

### 🎛️ 3. Les Tableaux de Bord (La Tour de Contrôle)

Le `/dashboard`, c'est la vue d'ensemble. Les (Gérant, Assistant Gérant, Employé ou Admin), se connecte, il ouvre ça, et il sait exactement quoi faire de sa journée sans fouiller dans les menus.

## Widget sur le dasbord :

Le contenu de ce tableau de bord **change selon le rôle** :

- **Vue Commis / Employé :**
    
    - _Widget Horaire :_ "Ton prochain quart de travail est demain à 9h00."
        
    - _Widget Transferts :_ "Il y a 2 boîtes à préparer pour la succursale B."
        
- **Vue Gérant / Admin :**
    
    - _Widget Alertes (via ton service Go) :_ "Attention : 5 livres sont en rupture de stock."
        
    - _Widget Stats rapides :_ "Valeur de l'inventaire : 45 000 $."
        
    - _Widget RH :_ "2 employés ont des congés à approuver."
        

**La règle d'or du Dashboard :** Les widgets ne sont pas des pages complètes pour faire du travail lourd. Ce sont des raccourcis. Si le Gérant clique sur "5 livres en rupture" dans son widget, ça le téléporte vers la vraie page `/inventaire/livres` avec le filtre "Rupture" déjà activé.

---

### 🛠️ 4. Les Composants Réutilisables (Les briques de construction)

Pour ne pas coder 100 fois la même chose, on va devoir fabriquer 3 briques de base dans React avant de faire les pages :

1. **Le `DataGrid` (Le super tableau) :** Un tableau intelligent capable de trier, filtrer et paginer, qu'on réutilisera pour les livres, les produits et les employés.
    
2. **Le `Modal` (Les fenêtres pop-up) :** Pour confirmer une suppression ou afficher un formulaire rapide sans changer de page.
    
3. **Les `FormControls` :** Des champs de texte standardisés avec gestion des erreurs (ex: encadré en rouge si le prix est manquant).
