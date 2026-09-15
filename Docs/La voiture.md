## La voiture

Le code vit dans `Assets/Scripts/Car/`.

### Principe

La voiture n'a qu'un contrôle : la direction. La vitesse avant ne lui appartient pas — c'est le monde qui glisse vers elle. Physiquement, son Rigidbody ne fait que tourner en yaw et glisser latéralement.

### Car

`Car` est le composant principal, monté sur un Rigidbody.

Ses références : l'action d'input de déplacement (l'axe X sert au braquage), le `RunManager` qui porte l'état de la run, les transforms des quatre roues et de la carrosserie. L'état de la run vit dans le `RunManager`, la voiture expose juste `State` et `ForwardSpeed` en passthrough.

Direction (`FixedUpdate`) :

1. L'input brut de l'axe X est lissé via `MoveTowards` : le braquage n'est pas instantané.
2. L'angle de braquage maximal est dynamique, il descend avec la vitesse (de `_maxDiagonalAngle` vers sa moitié) : plus on va vite, moins on est maniable.
3. `SmoothDampAngle` amène l'angle courant vers sa cible, borné par la vitesse angulaire maximale issue de la géométrie du véhicule — un bicycle model simplifié : `ForwardSpeed × tan(angle roue) / empattement`.
4. La vitesse angulaire Y est posée directement sur le Rigidbody, avec un realignement : une collision décale le yaw réel, l'erreur décroît à chaque tick (`RealignSpeed`) vers l'angle de braquage.
5. La vitesse latérale vaut `ForwardSpeed × tan(angle)` ; les composantes verticale et avant du Rigidbody sont conservées.

Visuel (`Update`) :

- Les roues avant tournent selon la vitesse angulaire courante (`atan(ω × empattement / vitesse)`) ; les arrière restent droites.
- Toutes les roues roulent en continu : `vitesse × dt / rayon de roue`.
- La carrosserie prend un roll proportionnel à l'angle des roues et à la vitesse, plus un shake de bruit de Perlin en rotation Z et position Y, qui s'accélère avec la vitesse. C'est ce shake qui donne la sensation de vitesse.

Collisions (`OnTriggerEnter`) :

1. Un `DeltaStat` est réinitialisé (une seule instance, réutilisée : aucune allocation par collision), puis l'entité touchée y applique ses dégâts via `OnCollide`.
2. Les items de l'inventaire de la run ajustent le delta via `OnInventory`, avant application.
3. `State.Apply(ref _delta)` applique le tout à l'état de la run.
4. Les effets de l'entité spawnent à sa position, attachés à son chunk : ils glissent avec le monde et survivent à l'entité.
5. L'entité retourne dans son pool.

### DeltaStat

Une struct simple : `Car`, `Health`, `Fuel`, `Distance`, `Score`, plus un `Reset`. C'est la monnaie d'échange des hooks : chaque hook reçoit le delta par référence et le modifie, et seul `State.Apply` l'applique réellement.

### InventorySlotCount

Une struct sérialisable qui dit combien d'emplacements existent pour un type d'item d'inventaire : `Slot` (`Engine`, `Wheels`, `Body`...) et `Amount`. Ça sert à configurer la capacité de l'inventaire par type.
