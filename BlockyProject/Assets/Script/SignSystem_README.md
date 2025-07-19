# Interactive Sign System Setup Guide

This guide will walk you through setting up the interactive sign system in your 2D game.

## 1. Set up the SignManager

1. Create an empty GameObject in your main scene and name it "SignManager"
2. Add the `SignManager.cs` script to this GameObject

## 2. Create a UI for displaying messages

1. Create a Canvas in your scene (GameObject -> UI -> Canvas)
2. Create a Panel as a child of the Canvas (GameObject -> UI -> Panel)
   - Name it "MessagePanel"
   - Position it where you want messages to appear (e.g., bottom of screen)
   - Adjust the size and appearance as desired
3. Add a Text - TextMeshPro UI element as a child of MessagePanel (GameObject -> UI -> Text - TextMeshPro)
   - Make sure TextMeshPro is imported in your project
   - Adjust font, size, and alignment as desired
   - Name it "MessageText"
4. Add the `SignPanelSetup.cs` script to the SignManager GameObject
   - This script will automatically wire up the UI elements to the SignManager

## 3. Create a Sign prefab

1. Create an empty GameObject and name it "Sign"
2. Add a SpriteRenderer component and assign a sign sprite
3. Add a BoxCollider2D or CircleCollider2D component
   - Make sure "Is Trigger" is checked
4. Add the `Sign.cs` script
5. Configure the Sign script settings in the Inspector:
   - Sign Message: Enter the text that will be displayed when interacting
   - Interaction Radius: Set the distance at which the player can interact with the sign
   - Player Layer: Set the layer that contains the player
6. (Optional) Add a visual interaction indicator:
   - Create a child GameObject with a sprite or UI element
   - Assign it to the "Interaction Indicator" field in the Sign component
   - This will appear when the player is in range
7. Make this GameObject a prefab by dragging it to your Project window

## 4. Place Signs in your Scene

1. Drag instances of your Sign prefab into your scene
2. Position them where you want signs to appear
3. Customize the message for each sign in the Inspector

## 5. Player Setup

1. Make sure your player GameObject has:
   - A Rigidbody2D
   - A Collider2D
   - Is on the layer specified in the Sign script
   - Has the "Player" tag

## 6. Input System Setup

The sign system supports both the new Unity Input System and legacy input:

### For New Input System:
1. Make sure your player has a PlayerInput component
2. Ensure the "Interact" action is defined in your input actions asset
3. Bind this action to the desired key/button (e.g., E, Space, or controller button)

### For Legacy Input:
1. The system will fall back to using the "Submit" button which is typically mapped to Enter or E
2. You can customize this in Edit -> Project Settings -> Input Manager

## Testing

1. Enter Play mode
2. Move your player character close to a sign
3. Press the interaction button when in range
4. The sign's message should appear on screen
5. The message will automatically disappear after the display time (configurable in SignManager)

## Customization Options

- **SignManager.cs**
  - Display Time: Change how long messages remain on screen
  - UI Customization: Modify the panel and text styling

- **Sign.cs**
  - Interaction Radius: Adjust how close the player needs to be
  - Visual Indicators: Add custom indicators or effects

Enjoy your interactive sign system!
