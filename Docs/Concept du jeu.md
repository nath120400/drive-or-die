## Concept du jeu

*Résumé du GDD, à destination des développeurs.*

### Principe

Drive or Die est un infinite runner post-apocalyptique. Le joueur est au volant d'une voiture qui roule toujours tout droit : pas de déplacement gauche/droite à gérer au-delà du contrôle latéral, et pas de direction à tenir. La caméra est une vue à la troisième personne, derrière la voiture, qui la suit en permanence.

### Objectif

Faire le meilleur score. Le jeu est itératif : chaque partie est une tentative de battre son record.

### Les deux jauges

La voiture vit sous deux jauges :

- Santé : la voiture se dégrade.
- Essence : la voiture consomme du carburant pour rouler.

Quand l'une des deux tombe à zéro, c'est game over. Ce sont les deux seules conditions de défaite.

### Les éléments de la route

La route est parsemée de différents éléments, dont voici des exemples :

- Les items de soin : foncer dedans répare la voiture.
- Les items d'essence : ils rajoutent du carburant.
- Les obstacles : zombies, carcasses de voitures...

### La partie role play

Le GDD contient une partie role play (lore, narration). Elle n'est pas implémentée dans le prototype : l'objectif est la jouabilité, uniquement.
