using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

/***************************************************
 * Interface class
 * instantiate in PrisonGame after player is instantiated
 * Arguments:
 *  -Game: the one and only prison game
 *  -Player: The player instantiation the interface passes commands to. 
 *      Should be the player instantiated directly before this!
 *  -playerControllerIndex: The controller number we take input from. EG: Controller 1
 *  */


namespace PrisonStep
{
    public class Interface
    {
        /// <summary>
        /// The game in which this interface exists
        /// </summary>
        PrisonGame game;

        /// <summary>
        /// The player instantiation this interface controls
        /// </summary>
        Player player;

        /// <summary>
        /// The player index to indicate what controller to take instruction from
        /// </summary>
        PlayerIndex index;

        /// <summary>
        /// Holds the last state the gamepad was in
        /// </summary>
        GamePadState lastGamepadState;


        public Interface(PrisonGame game, Player player, int playerControllerIndex)
        {
            this.game = game;
            this.player = player;

            if (playerControllerIndex == 1)
            {
                index = PlayerIndex.One;
            }
            else if (playerControllerIndex == 2)
            {
                index = PlayerIndex.Two;
            }
            else if (playerControllerIndex == 3)
            {
                index = PlayerIndex.Three;
            }
            else if (playerControllerIndex == 4)
            {
                index = PlayerIndex.Four;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(index).Triggers.Right > 0)
            {
                //type is float
                //Call a function from the player to shoot a laser
                //pass float to function
            }

            if (GamePad.GetState(index).Triggers.Left > 0)
            {
                //type is float
                //Call a function from the player to raise a shield
                //pass float to function
            }

            if (GamePad.GetState(index).ThumbSticks.Right != Vector2.Zero)
            {
                //type is vector2
                //Call a function from the player change the camera angle
                //pass Xfloat, Yfloat, and gameTime to function
            }

            if (GamePad.GetState(index).ThumbSticks.Left != Vector2.Zero)
            {
                //type is vector2
                //Call a function from the player to move themself
                //pass Xfloat, Yfloat, and gameTime to function
            }

            if (GamePad.GetState(index).DPad.Left != ButtonState.Pressed
                && lastGamepadState.DPad.Left != ButtonState.Pressed)
            {
                //no type to pass
                //Call a function from the player to make them switch to element 1
            }

            if (GamePad.GetState(index).DPad.Up != ButtonState.Pressed
                && lastGamepadState.DPad.Up != ButtonState.Pressed)
            {
                //no type to pass
                //Call a function from the player to make them switch to element 2
            }

            if (GamePad.GetState(index).DPad.Right != ButtonState.Pressed
                && lastGamepadState.DPad.Right != ButtonState.Pressed)
            {
                //no type to pass
                //Call a function from the player to make them switch to element 3
            }

            if (GamePad.GetState(index).Buttons.A == ButtonState.Pressed
                && lastGamepadState.Buttons.A != ButtonState.Pressed)
            {
                //no type to pass
                //Call a function from the player to yell "Exterminate!"
            }

            if (GamePad.GetState(index).Buttons.B != ButtonState.Pressed
                && lastGamepadState.Buttons.B != ButtonState.Pressed)
            {
                //no type to pass
                //Call a function from the player to make them cast thier selected element
            }

            lastGamepadState = GamePad.GetState(index);

        }
    }
}
