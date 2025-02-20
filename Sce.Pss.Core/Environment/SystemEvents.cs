using System;
using System.Diagnostics;
using System.Threading;
using Sakura;
using Sce.Pss.Core.Graphics;

/*
About the PC Simulator

Keys on the gamepad	Key assignments on the PC simulator
Left directional key	Cursor key: ←
Up directional key	Cursor key: ↑
Right directional key	Cursor key: →
Down directional key	Cursor key: ↓
Square button	Alphabet: A
Triangle button	Alphabet: W
Circle button	Alphabet: D
Cross button	Alphabet: S
SELECT button	Alphabet: Z
START button	Alphabet: X
L button	Alphabet: Q
R button	Alphabet: E
*/
namespace Sce.Pss.Core.Environment
{
	public static class SystemEvents
	{
		private static Stopwatch __timer = new Stopwatch();
		private static bool __isMouseLeftDown = false;
		
		public static void CheckEvents ()
		{
			SakuraGameWindow.ProcessEvents();
			
			bool __isWinFocused = SakuraGameWindow.getFocused();
            OpenTK.Input.KeyboardState keyboard = OpenTK.Input.Keyboard.GetState();

			if (__isWinFocused)
			{
				if (keyboard.IsKeyDown(OpenTK.Input.Key.Escape))
	            {
	            	System.Environment.Exit(0);
	            }
				//  
			}
			else
			{
				
			}
            
            
            if (__isWinFocused)
			{
	            //OpenTK.Input.MouseState mouse = OpenTK.Input.Mouse.GetState();
	            OpenTK.Input.MouseState mouse = OpenTK.Input.Mouse.GetCursorState();
	            OpenTK.Point pt = SakuraGameWindow.PointToClient(new OpenTK.Point(mouse.X, mouse.Y));
	            float winW = SakuraGameWindow.getWidth();
				float winH = SakuraGameWindow.getHeight();
		        if (mouse.IsButtonUp(OpenTK.Input.MouseButton.Left))
	            {
	            	if (__isMouseLeftDown == true)
	            	{
		            	//Debug.WriteLine("down:" + pt.X + "," + pt.Y);
	            	}
	            	__isMouseLeftDown = false;          	
	            }
	            //OpenTK.WindowState wState = MyGameWindow.getWindowState();
	            //wState != OpenTK.WindowState.Minimized
	            if (mouse.IsButtonDown(OpenTK.Input.MouseButton.Left))
	            {
		            if (__isMouseLeftDown == false)
		            {
		            	
		            } else {
		            	
		            }
		            __isMouseLeftDown = true;
		        }
            }
            else
            {
            	__isMouseLeftDown = false;
            }
            
#if false
			double delta = __timer.Elapsed.TotalMilliseconds;
			double frame = 1000.0 / 24.0;
			if (delta < frame)
			{
				int free = (int)(frame - delta);
				Thread.Sleep(free);
				//Debug.WriteLine("Sleep: " + free);
			}
			__timer.Restart();
#endif
		}
		
//		public delegate void RestoredEventHandler (object sender, RestoredEventArgs e);
//		
//		public static event RestoredEventHandler OnRestored;
//		public static void OnRestoredHandle()
//		{
//			OnRestored(null, new RestoredEventArgs());
//		}
	}
}
