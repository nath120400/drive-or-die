## Entités

Tout ce qui se trouve sur la route est une entité : items de soin, jerricans d'essence, zombies, carcasses de voiture... Le code vit dans `Assets/Scripts/Entity/`.

Le système est data-driven : chaque type d'entité est une asset qui décrit tout ce qu'il y a à savoir sur elle, et le runtime ne manipule que ces données.

### EntityDescription

Le cœur du système : une classe abstraite héritant de `ScriptableObject`. Chaque type d'entité correspond à une asset qui porte toutes ses données.

D'un côté les données d'affichage :

- `Description` : le texte descriptif.
- `Icon` : l'icône.
- `Prefab` : ce qui est instancié en jeu.
- `Effects` : les effets visuels associés.

De l'autre les stats de gameplay :

- `MinWeight` / `MaxWeight` : bornes du poids d'apparition. Le poids effectif interpole entre les deux selon la difficulté de la run.
- `Radius` : le rayon de l'entité.
- `InventorySlot` : l'emplacement d'inventaire auquel l'entité appartient.
- `Biome` : le biome de l'entité.
- `Type` : le type (`Collidable` ou `Pickable`).

Le pooling est géré par la description elle-même, via une stack de `GameObject`s :

- `Spawn(position, chunk)` dépile une instance libre, ou instancie le prefab — toujours sous le transform du chunk — puis l'active et renvoie son composant `Entity`.
- `Release(entity)` fait l'inverse : détache l'entité de son chunk, désactive le GameObject, rempile.

Enfin, quatre hooks virtuels, no-op par défaut, servent de point d'extension :

- `OnEntityBlueprint` : appelé pendant la génération, pour ajuster le poids du blueprint.
- `OnCollide` : appelé quand le joueur touche l'entité.
- `OnInventory` : appelé quand un item équipé modifie la run.
- `OnScore` : appelé sur le score.

Chaque sous-classe définit son comportement en modifiant le `DeltaStat` reçu par référence.

### Entity

Le composant MonoBehaviour monté sur chaque instance en scène. Il ne porte aucune logique, juste deux références : `Description` (d'où vient l'instance) et `Chunk` (où elle se trouve).

### EntityBlueprint

Une struct légère : un poids courant (après filtres et difficulté) et une référence vers la description. C'est la représentation intermédiaire entre la liste globale d'entités et le spawn effectif — la génération manipule des blueprints, jamais des GameObjects.

### EntityDatabase

Un ScriptableObject contenant la liste globale des descriptions. C'est la source unique que la génération parcourt.

### Enums

Trois enums qualifient les entités :

- `Biome` : `None`, `Desert`, `City`, `Country`.
- `EntityType` : `Collidable` (obstacle) / `Pickable` (item à ramasser).
- `InventorySlot` : `None`, `Engine`, `Wheels`, `Body`.

### Un exemple concret : InstantDamage

`InstantDamage` hérite d'`EntityDescription`, expose un champ `_damage` et surcharge `OnCollide` pour décrémenter `delta.Health`. C'est tout ce qu'il faut pour faire une entité : une asset et un hook.

### Pourquoi cette architecture

- **Data-driven** : ajouter une entité dont le comportement est déjà couvert ne demande aucun code, juste une asset dans l'éditeur.
- **Extensible** : un comportement particulier se fait en surchargeant un hook dans une sous-classe, sans toucher à la génération ni au gameplay.
- **Découplé** : la donnée (`EntityDescription`), l'instance en scène (`Entity`) et le blueprint de génération (`EntityBlueprint`) sont trois objets distincts ; la génération travaille sur des objets légers.
- **Performant** : le pooling évite les instanciations/destructions répétées, point sensible pour un runner qui spawne en continu.
- **Local** : les entités vivent dans leur chunk, libérer un chunk libère tout son contenu sans connaître chaque entité.
