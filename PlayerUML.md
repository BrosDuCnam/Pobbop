````mermaid
classDiagram
Controller <|-- PlayerInputController
Controller <|-- BotController

BasePlayer *-- TargetSystem
BasePlayer *-- ThrowSystem
BasePlayer *-- PickUpDropSystem
BasePlayer *-- HealthSystem

BasePlayer <|-- RealPlayer
BasePlayer <|-- BotPlayer

RealPlayer *-- PlayerInputController
BotPlayer *-- BotController
````