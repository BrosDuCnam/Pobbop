````mermaid
classDiagram
Controller <|-- PlayerInputController
Controller <|-- BotController

Player *-- Targetter
Player *-- Throw
Player *-- PickUp

Player <|-- RealPlayer
Player <|-- BotPlayer

RealPlayer *-- PlayerInputController
BotPlayer *-- BotController

FSMachine *-- BotPlayer

FSMStateInfo <|-- SBStateInfo
FSMState <|-- SBStateInfo

FSMState <|-- SBSBase
FSMState <|-- SBSNavigate
FSMState <|-- SBSBallChasing
FSMState <|-- SBSShoot
FSMState <|-- SBSPlayerChasing
FSMState <|-- SBSHasBall
````