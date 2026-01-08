module Client.GameDomain

open System

// GRID GAMES THAT USE LIST OF LANE OBJECTS TO DEFINE THEIR GRID BOARD AND CONTENTS
module GridGame =

    let rand = Random()

    // return random number within ceiling
    let randomIndex maxNum = rand.Next(maxNum)

    // Grid Movement Directions
    type MovementDirection =
        | Up
        | Down
        | Left
        | Right

    type TapTileValue =
        | Minor
        | Modest
        | Major
        | Heart
        | Bomb

    type TapTile = {
        TapPosition: int
        LifeTime: int
        Value: TapTileValue
    }

    type TileSortValueTile = { Value : int option }

    // Object that can be in any given space
    // THIS SHOULD BE MADE GENERIC WHERE THE SUB IMPLEMENTS
    type LaneObject =
        | Ball
        | Blank
        | Blocker
        | Bomb
        | Goal
        | Heart
        | LaneLock
        | LaneKey
        | MoveArrow of MovementDirection
        | TileSortLaneObject of TileSortValueTile
        | TapTile of TapTile

    // List of LaneObjects that occupy a Grid Position
    type GridBoard = {
        GridPositions: LaneObject list
    }

    // Represents a given game state
    type RoundState =
        | Settings
        | Paused // change to idle (no game active)
        | Playing // there is an active round
        | Won // Round ended, model contains round details

    // unwrap optional int from tryFindIndex
    // this is pretty unnecessary and should be 'baked' into the model domain
    let unwrapIndex index =
        match index with
        | Some i -> i
        | _ -> -1

    // returns true or false based on if the lookupObject is at the gridPositionIndex
    let checkGridPositionForObject positions (gridPositionIndex: int) objToFind =
        positions.GridPositions.Item(gridPositionIndex) = objToFind

    // returns the object at a given positions index
    let getObjectAtPositionIndex (positions: LaneObject list) (positionIndex: int) =
        positions.Item(positionIndex)

    // would return the first index if one is found
    let getObjectPositionIndex (positions: GridBoard) (positionObject: LaneObject) =
        List.tryFindIndex (fun x -> x = positionObject ) positions.GridPositions

    // updates the grid position with an object
    let updatePositionWithObject positions (object: LaneObject) position =
        let gridLength = positions.GridPositions.Length - 1
        let newGrid =
            // [ for i in 0 .. gridLength do
            [ for i in 0 .. gridLength do
                if i = position
                    then object
                    else positions.GridPositions.Item(i)
            ]
        {GridPositions = newGrid}

    // GRID POSITION REPRESENTATION
    // GridPositions Represented in column groups
    let getPositionsAsColumns (positions: GridBoard) gridDimension =
        Seq.toList (seq {
            for i in 0 .. (gridDimension - 1) do
                yield Seq.toList (seq {
                    for n in 0 .. (gridDimension - 1) do
                        yield! seq{
                            positions.GridPositions.Item(i + (n * gridDimension))
                        }
                })
        })

    // GridPositions Represented in row groups
    let getPositionsAsRows (positions: GridBoard) gridDimension =
        Seq.toList (seq {
            for i in 0 .. (gridDimension - 1) do
                yield Seq.toList (seq {
                    for n in 0 .. (gridDimension - 1) do
                        yield! seq{
                            positions.GridPositions.Item(n + (i * gridDimension))
                        }
                })
        })

    let modelValueAsString strin value =
        if value = -1
            then strin + "\u221E";
            else strin + string value

module RollableGridGameHelpers =

    open GridGame

    let getPiecePositionIndex ( gameGridPositions: GridBoard ) positionObject =
        getObjectPositionIndex gameGridPositions positionObject
        |> unwrapIndex

    let getRolledBallPositionIndex wrapped ballPosition direction =
        match wrapped, direction with
        | false, MovementDirection.Up -> (ballPosition - 8)
        | true, MovementDirection.Up ->  ballPosition + 56
        | false, MovementDirection.Down -> (ballPosition + 8)
        | true, MovementDirection.Down ->  ballPosition - 56
        | false, MovementDirection.Left -> (ballPosition - 1)
        | true, MovementDirection.Left ->  ballPosition + 7
        | false, MovementDirection.Right -> (ballPosition + 1)
        | true, MovementDirection.Right ->  ballPosition - 7

    let checkDirectionalBound ballRollPositionIndex direction =
        match direction with
        | MovementDirection.Up -> ballRollPositionIndex >= 0
        | MovementDirection.Down -> ballRollPositionIndex <= 63
        | MovementDirection.Right -> ((ballRollPositionIndex) % 8) <> 0
        | MovementDirection.Left -> ((ballRollPositionIndex + 1) % 8) >= 1

    let checkGridBoardPositionForBlocker gridBoard positionIndex =
        gridBoard.GridPositions.Item ( positionIndex ) = Blocker

    let updateGameBoardMovedPosition gameBoard piecePosition movedPiecePosition =
        let ballToBlankGrid = updatePositionWithObject (gameBoard) Blank (piecePosition)
        updatePositionWithObject ballToBlankGrid Ball movedPiecePosition

    let nonWrappingPieceMovementPositionIndex ballPosition direction =
        getRolledBallPositionIndex false ballPosition direction

    let wrappablePieceMovementPositionIndex ballPosition direction =
        checkDirectionalBound ( getRolledBallPositionIndex false ballPosition direction ) direction
        |> fun x -> getRolledBallPositionIndex ( not x ) ballPosition direction

    let checkDirectionMovementWithFunction movementPositionFunc ballPosition direction gameBoard =
        let updatedPiecePosition = movementPositionFunc ballPosition direction
        if checkGridBoardPositionForBlocker gameBoard updatedPiecePosition
            then gameBoard
            else updateGameBoardMovedPosition gameBoard ballPosition updatedPiecePosition

    let checkRollableDirectionMovement ballPosition direction gameBoard =
        checkDirectionMovementWithFunction nonWrappingPieceMovementPositionIndex ballPosition direction gameBoard

    let checkDirectionMovement ballPosition direction gameBoard =
        checkDirectionMovementWithFunction wrappablePieceMovementPositionIndex ballPosition direction gameBoard
