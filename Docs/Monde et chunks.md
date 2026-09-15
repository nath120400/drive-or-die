## Monde et chunks

La route infinie repose sur des chunks : des segments de route recyclés en boucle. Le code vit dans `Assets/Scripts/World/`.

### Principe

Le `ChunkManager` maintient un ring de chunks devant la voiture. Le monde glisse vers l'arrière ; quand le chunk de tête passe derrière la voiture, son contenu est libéré dans les pools et le chunk est replacé en fin de file, puis re-seedé.

### Chunk et ChunkZone

`Chunk` est le MonoBehaviour d'un segment de route :

- `Zone` : la `ChunkZone` du prefab.
- `Entities` : les entités rattachées au chunk.
- `Length` : la longueur du chunk, dérivée de la profondeur de la zone (`Zone.Max.y - Zone.Min.y`).

`ChunkZone` définit la zone de spawn dans le repère du marqueur — l'objet de scène placé dans le prefab de chunk :

- `Min` / `Max` : le rectangle horizontal.
- `Altitude` : la hauteur de base.

C'est le marqueur qui commande tout : son rectangle sert à diviser la zone en cellules, et son tilt/rotation se répercute sur la position des entités. Un gizmo jaune (`OnDrawGizmosSelected`) le dessine dans l'éditeur.

### ChunkManager

Le composant qui orchestre tout.

Paramètres :

- `AheadChunks` : nombre de chunks maintenus devant la voiture.
- `ChunkPrefabs` : les variants de chunks, tirés au hasard.
- `Database` : la `EntityDatabase`.
- `XCells` / `YCells` : la zone est divisée en une grille de cette taille.
- `Tries` : essais de placement par entité.
- `Border` : marge entre deux entités voisines.
- `MinTarget` / `MaxTarget` : budget d'entités par chunk, interpolé selon la difficulté de la run.
- `_emptyChunksAtStart` : chunks vides au lancement, pour démarrer sur une route dégagée.

Cycle de vie :

- Au lancement (`OnEnable`), les `AheadChunks` chunks sont créés et seedés d'un coup ; le recyclage ne démarre qu'une fois le ring rempli (`_initialized`).
- Chaque frame, tout le monde glisse vers l'arrière à la vitesse de la voiture.
- Quand le chunk de tête passe derrière le zéro local, ses entités sont libérées (`Description.Release`, en parcourant la liste à l'envers), puis le chunk repart en queue via `NewChunk`.
- `CreateChunk` instancie un prefab variant au hasard.

Génération du contenu (`NewChunk`) :

Chaque chunk re-seedé reconstruit sa liste de blueprints :

1. Chaque description de la base devient un `EntityBlueprint`, dont le poids interpole entre `MinWeight` et `MaxWeight` selon la difficulté.
2. `FilterBlueprint` est appliqué sur chaque blueprint (vide pour l'instant, prévue pour les filtres futurs).
3. Les items de l'inventaire de la run ajustent les poids via leur hook `OnEntityBlueprint`.
4. Les blueprints de poids ≤ 0 sont jetés.
5. Le total des poids sert au tirage pondéré (`DrawBlueprint`).

Placement (dart throwing) :

- Le chunk doit recevoir `Target` entités. Pour chacune, on tire une cellule de la grille au hasard, avec `Tries` essais si elle est occupée. Une cellule, une entité.
- La grille est une liste plate (index = `row * cols + col`), allouée une fois et vidée à chaque chunk : zéro allocation.
- Chaque rangée est décalée latéralement d'au plus une cellule : sans ça, les entités s'aligneraient en colonnes visibles. La colonne du bord, côté décalage, est vidée pour que rien ne dépasse de la route.
- Une fois la grille remplie, la conversion en positions : jitter dans la cellule, réduit de `Border` (deux voisines jamais plus proches que `2 × Border`), altitude de la zone, transform du marqueur vers celui du chunk.
- Rotation aléatoire sur Y pour chaque entité.

### Pourquoi cette architecture

- **Zéro GC** : le ring recycle les chunks et la grille est réutilisée. Après le lancement, plus rien n'est créé ni détruit en jeu.
- **Répartition naturelle** : le dart throwing donne un aléatoire sans amas ni alignements, à coût constant. Un tirage naïf par position produirait des chevauchements et des rangées denses.
- **Jouabilité garantie** : espacement minimal et interdiction de déborder de la route, sans post-traitement.
- **Difficulté par la donnée** : la difficulté ne courbe que deux nombres, le budget (densité) et les poids (fréquences). Tout le tuning se fait dans les assets (`MinWeight`/`MaxWeight`).
- **Contenu découplé du terrain** : ajouter un variant de chunk se résume à un prefab avec son marqueur ; la génération ne connaît pas les prefabs, elle lit la zone.
- **Chunks indépendants** : chaque chunk est seedé sans dépendance vis-à-vis de ses voisins, donc recyclable n'importe où dans le ring.
