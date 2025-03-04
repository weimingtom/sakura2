﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES20;

using Sakura;

//using Sce.Pss.Core.Imaging;

namespace Sce.Pss.Core.Graphics
{
	public class GraphicsContext : IDisposable
	{
		//see https://stackoverflow.com/questions/250404/where-does-console-writeline-go-in-debug
		public class __DebugTextWriter : TextWriter
	    {
	        public override Encoding Encoding
	        {
	            get { return Encoding.UTF8; }
	        }
	
	        //Required
	        public override void Write(char value)
	        {
	            Debug.Write(value);
	        }
	
	        //Added for efficiency
	        public override void Write(string value)
	        {
	            Debug.Write(value);
	        }
	
	        //Added for efficiency
	        public override void WriteLine(string value)
	        {
	            Debug.WriteLine(value);
	        }
	    }
		
		
		
		
		//see http://blog.csdn.net/hb707934728/article/details/52044702
		//http://www.opengl-tutorial.org
		//https://github.com/opengl-tutorials/ogl
		//https://github.com/ynztlxdeai/GLproject/tree/master/app/src/main/assets
		//http://doc.qt.io/qt-5/qtgui-openglwindow-example.html
		
		public static Dictionary<int, bool> __isUsedProgram = new Dictionary<int, bool>();
		public static Dictionary<int, ShaderProgram> __programDic = new Dictionary<int, ShaderProgram>();
		public static Dictionary<int, VertexBuffer> __vertexBuffer = new Dictionary<int, VertexBuffer>();
//		public static Dictionary<int, Texture> __textureDic = new Dictionary<int, Texture>();
		private FrameBuffer __frameBuffer;
		private FrameBuffer __screen;
	    private static readonly float[] __vVertices = {  
        	0.0f,  0.5f, 0.0f,
	        -0.5f, -0.5f, 0.0f,
	        0.5f, -0.5f, 0.0f 
        };		
		
		public GraphicsContext()
		{
			Console.SetOut(new __DebugTextWriter());
			System.IO.Directory.CreateDirectory("./Documents");
			System.IO.Directory.CreateDirectory("./save");
			
			SakuraGameWindow.Init();
//			SakuraGameWindow.Restore += delegate(object sender, EventArgs e) { 
//				Sce.Pss.Core.Environment.SystemEvents.OnRestoredHandle();
//				//Debug.WriteLine("=============>");
//			};
			
			Color4 color = Color4.Black;//FIXME:background
            //color = Color4.MidnightBlue;
            GL.ClearColor(color.R, color.G, color.B, color.A);
            GL.Enable(EnableCap.DepthTest); //FIXME:
            //see http://tiankefeng0520.iteye.com/blog/2008008
            GL.DepthFunc(DepthFunction.Lequal);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest); 
            Clear();
			SwapBuffers();
			
			//For a little long time, so do it after SwapBuffers for showing black background at once
            //SakuraSoundManager.Initialize();
			
            this.__screen = FrameBuffer.__getScreen();
            this.__frameBuffer = this.__screen;
            
            string extensions = GL.GetString(StringName.Extensions);
            Debug.WriteLine(">>>extensions = " + extensions);
            if (extensions.Contains("GL_OES_texture_npot"))
            {
            	Debug.WriteLine(">>>Support NPOT");
            }
            else
            {
            	Debug.WriteLine(">>>Not support NPOT");            	
            }
        }
		
		public void Dispose ()
		{
			//SakuraSoundManager.Shutdown();
		}
		
		public FrameBuffer Screen
		{
			get
			{
				return __screen;//this.screen;
			}
		}

		public void SetViewport(int x, int y, int w, int h)
		{
			GL.Viewport(0, 0, w, h);
        }
		
		private Vector4 __curClearColor;
		public void SetClearColor(float r, float g, float b, float a)
		{
			GL.ClearColor(r, g, b, a);
			__curClearColor = new Vector4(r, g, b, a);
		}
		
		public void SetClearColor(Vector4 color)
		{
			GL.ClearColor(color.X, color.Y, color.Z, color.W);
			__curClearColor = color;
		}
		
		public void Clear()
		{
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		}
		
		private int __curProgramObject = 0;
		private ShaderProgram __curProgram = null;
		public void SetShaderProgram(ShaderProgram program)
		{
			if (program != null)
			{
				program.__linkProgram();
				__curProgramObject = program.__programObject;
				__curProgram = program;
				if (program.__programObject != 0)
				{
					__isUsedProgram[program.__programObject] = true;
					__programDic[program.__programObject] = program;
					__vertexBuffer.Clear(); //FIXME:?????? clear, because __programObject in UseProgram(program.__programObject) is changed
					//__textureDic.Clear(); //FIXME:?????? clear, because __programObject in UseProgram(program.__programObject) is changed
					//GL.Clear(ClearBufferMask.DepthBufferBit); //FIXME:???
					GL.UseProgram(program.__programObject);
		        	foreach (int location in program.__uniformMatrix4.Keys)
		        	{
		        		OpenTK.Matrix4 v = program.__uniformMatrix4[location];
		        		GL.UniformMatrix4 (location, false, ref v);
	//	        		if (program.__filename.Equals("/Application/Sample/Graphics/ShaderCatalogSample/shaders/Simple.cgx"))
	//	    			{
	//	        			Debug.WriteLine("======================location2:" + location + " : " + v.ToString());
	//	        		}
		        	}
		        	foreach (int location in program.__uniform4.Keys)
		        	{
		        		OpenTK.Vector4 v = program.__uniform4[location];
		        		GL.Uniform4 (location, v.X, v.Y, v.Z, v.W);
		        	}
		        	foreach (int location in program.__uniform3.Keys)
		        	{
		        		OpenTK.Vector3 v = program.__uniform3[location];
		        		GL.Uniform3 (location, v.X, v.Y, v.Z);
		        	}
		        	foreach (int location in program.__uniform1.Keys)
		        	{
		        		float v = program.__uniform1[location];
		        		GL.Uniform1 (location, v);
		        	}
		        	program.__afterUseProgram();
		        }
				else
				{
					//FIXME:
				}
			}
			else
			{
				__curProgramObject = 0; //FIXME:
				__curProgram = null;
			}
		}
		
		public void SetVertexBuffer(int index, VertexBuffer buffer)
		{
			//FIXME:index not unsed
			__vertexBuffer[index] = buffer;
		}
		
		public void DrawArrays(DrawMode mode, int first, int count)
		{
//			foreach (int index in __textureDic.Keys)
//			{
//				Texture texture = __textureDic[index];
//				this.__SetTexture(index, texture);
//			}
			
			//VertexAttribPointer
			//EnableVertexAttribArray
			foreach (VertexBuffer buffer in __vertexBuffer.Values)
			{
				for (int i = 0; i < buffer.__verticesArr.Length; ++i)
				{
					if (buffer.__verticesArr[i] == null)
					{
						break;
					}
					if (buffer.__formatsArr[i] == VertexFormat.Float3 || 
					    buffer.__formatsArr[i] == VertexFormat.Float4 ||
					    buffer.__formatsArr[i] == VertexFormat.Float2)
					{
						int size = 4;
						switch(buffer.__formatsArr[i])
						{
						case VertexFormat.Float3:
							size = 3;
							break;
							
						case VertexFormat.Float4:
							size = 4;
							break;
							
						case VertexFormat.Float2:
							size = 2;
							break;
							
						default:
							Debug.Assert(false);
							break;
						}
						VertexAttribPointerType vType = VertexAttribPointerType.Float;
						float[] vert = (float[])buffer.__verticesArr[i];
						if (vert == null)
						{
							Debug.Assert(false);
						}
						GL.VertexAttribPointer (i, size, vType, false, 0/*size * 4*/, vert);
						GL.EnableVertexAttribArray ( i );
					}
					else
					{
						Debug.Assert(false);
					}
				}
				
				//DrawArrays
				PrimitiveType type = PrimitiveType.Triangles;
				switch (mode)
				{
					case DrawMode.TriangleStrip:
						type = PrimitiveType.TriangleStrip;
						break;
						
					case DrawMode.Triangles:
						type = PrimitiveType.Triangles;
						break;
						
					case DrawMode.TriangleFan:
						type = PrimitiveType.TriangleFan;
						break;
						
					case DrawMode.Points:
						type = PrimitiveType.Points;
						break;
						
					case DrawMode.Lines:
						type = PrimitiveType.Lines;
						break;
						
					case DrawMode.LineStrip:
						type = PrimitiveType.LineStrip;
						break;
						
					default:
						Debug.Assert(false);
						break;
				}
				ushort[] indics = buffer.__indices;
				int indexCount = buffer.__indexCount;
				if (indexCount <= 0)
				{
					GL.DrawArrays(type, first, count);
				}
				else
				{
					GL.DrawElements(type, count, DrawElementsType.UnsignedShort, indics);
				}
				
				if (buffer != null)
				{
					for (int i = 0; i < buffer.__verticesArr.Length; ++i)
					{
						if (buffer.__verticesArr[i] == null)
						{
							break;
						}
						if (buffer.__formatsArr[i] == VertexFormat.Float3 || 
						    buffer.__formatsArr[i] == VertexFormat.Float4 ||
						    buffer.__formatsArr[i] == VertexFormat.Float2)
						{
							GL.DisableVertexAttribArray ( i );
						}
						else
						{
							Debug.Assert(false);
						}
					}
				}
			}
			
//			if (__disableBlend)
//			{
//				GL.Enable(EnableCap.DepthTest); //FIXME:???
//				__disableBlend = false;
//			}
   		}
		
		public void SwapBuffers()
		{  
#if true
			SakuraGameWindow.OnResize();
#endif
			SakuraGameWindow.SwapBuffers();
            List<int> akeys = new List<int>(__isUsedProgram.Keys);  
            foreach (int key in akeys)
            {
            	__isUsedProgram[key] = false;
            }
//            foreach (ShaderProgram pro in __programDic)
//            {
//            	pro.__uniform1.Clear();
//            	pro.__uniform2.Clear();
//            	pro.__uniform3.Clear();
//            	pro.__uniform4.Clear();
//            	pro.__uniformMatrix4.Clear();
//            	pro.__uniformDic.Clear();
//            }
            __programDic.Clear();
            //__useProgramBeforeSetTexture = false;
//            __textureDic.Clear();
		}
		
		public FrameBuffer GetFrameBuffer()
		{
			return __frameBuffer;
		}
		
		//private bool __useProgramBeforeSetTexture = false; //FIXME:???
//		public void SetTexture(int index, Texture texture)
//		{
////			if (index == 1)
////			{
////				Debug.WriteLine("====================1");
////			}
////			if (texture is Texture2D)
////			{
////				Texture2D t2 = (Texture2D)texture;
////				t2.__saveToFile2("xxx.png");
////				t2.__supportNPOT = true;
////			}
//			__textureDic[index] = texture;
//		}

//		private void __SetTexture(int index, Texture texture)
//		{
//			//FIXME:???SetShaderProgram
////			if (__useProgramBeforeSetTexture == false)
////			{
////				if (__curProgram != null)
////				{
////					SetShaderProgram(__curProgram);
////				}
////				__useProgramBeforeSetTexture = true;
////			}
//			
//			//FIXME:tex
//		   //glActiveTexture ( GL_TEXTURE0 );
//		   //glBindTexture ( GL_TEXTURE_2D, userData->textureId );
//			
//		   // Set the sampler texture unit to 0
//		   //glUniform1i ( userData->samplerLoc, 0 );
//		   TextureUnit textureUnit = TextureUnit.Texture0;
//		   switch (index)
//		   {
//		    case 0:
//		   		textureUnit = TextureUnit.Texture0;
//		   		break;
//		   
//		   	case 1:
//		   		textureUnit = TextureUnit.Texture1;
//		   		break;
//		   		
//		   	default:
//		   		Debug.Assert(false);
//		   		break;
//		   }
//		   Debug.Assert(((Texture2D)texture).__textureId >= 0);
//		   GL.ActiveTexture(textureUnit);
//		   GL.BindTexture (TextureTarget.Texture2D, ((Texture2D)texture).__textureId); //FIXME:
//		   switch (texture.__wrap)
//		   {
//				case TextureWrapMode.ClampToEdge:
//					//FIXME:???
//				   	GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
//					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);							
//					break;
//					
//				case TextureWrapMode.Repeat:
//					//FIXME:???
//					if (true)
//					{
//						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.Repeat);
//						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.Repeat);	
//					}
////					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
////					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);					
//					break;
//					
//				default:
//					Debug.Assert(false);
//					break;
//		   }
//		   
//		   int samplerLoc = GL.GetUniformLocation(__curProgramObject, "Texture" + index );
////		   Debug.WriteLine("program: " + __curProgramObject + ", SetTexture:uniform['" + "Texture" + index + "':].location == " + samplerLoc);
////		   if (index == 1)
////		   {
////		   		Debug.WriteLine("=============1");
////		   }
//		   GL.Uniform1(samplerLoc, index);
//		}
		
//		public void Enable(EnableMode mode)
//		{
//			switch (mode)
//			{
//				case EnableMode.Blend:
//					GL.Enable(EnableCap.Blend);
//					//FIXME:???for sprite black bg problem
//					//TODO:removed, not safe
////					GL.Disable(EnableCap.DepthTest); //FIXME:???
////					__disableBlend = true;
//					break;
//					
//				case EnableMode.DepthTest:
//					GL.Enable(EnableCap.DepthTest);
//					break;
//				
//				case EnableMode.CullFace:
//					GL.Enable(EnableCap.CullFace);
//					break;
//					
//				default:
//					Debug.Assert(false);
//					break;
//			}
//		}
//		
//		//private bool __disableBlend = false;
//		public void Enable (EnableMode mode, bool status)
//		{
//			//Debug.Assert(false);
//			EnableCap mode_ = 0; //FIXME:
//			switch (mode)
//			{
//				case EnableMode.CullFace:
//					mode_ = EnableCap.CullFace;
//					break;
//					
//				case EnableMode.DepthTest:
//					mode_ = EnableCap.DepthTest;
//					break;
//					
//				case EnableMode.Blend:
//					mode_ = EnableCap.Blend;
//					//FIXME:??????for sprite black bg problem
//					//TODO:removed, not safe
////					if (status == true)
////					{
////						GL.Disable(EnableCap.DepthTest); //FIXME:???
////						__disableBlend = true;
////					}
//					break;
//				
//				case EnableMode.ScissorTest:
//					mode_ = EnableCap.ScissorTest;
//					break;
//					
//				case EnableMode.StencilTest:
//					mode_ = EnableCap.StencilTest;
//					break;
//					
//				default:
//					Debug.Assert(false);
//					break;
//			}
//			if (status)
//			{
//				GL.Enable(mode_);
//			}
//			else
//			{
//				GL.Disable(mode_);
//			}
//		}
		
//		private BlendFunc __blendFunc;
//		
//		public void SetBlendFunc(BlendFuncMode mode, BlendFuncFactor srcFactor, BlendFuncFactor dstFactor)
//		{
//			__blendFunc = new BlendFunc(mode, srcFactor, dstFactor);
//			
//			BlendEquationMode _mode = BlendEquationMode.FuncAdd;
//			switch (mode)
//			{
//				case BlendFuncMode.Add:
//					_mode = BlendEquationMode.FuncAdd;
//					break;
//
//				default:
//					Debug.Assert(false);
//					break;
//			}
//			BlendingFactorSrc _src = BlendingFactorSrc.SrcAlpha;
//			BlendingFactorDest _dst = BlendingFactorDest.OneMinusSrcAlpha;
//			switch (srcFactor)
//			{
//				case BlendFuncFactor.Zero:
//					_src = BlendingFactorSrc.Zero;
//					break;
//					
//				case BlendFuncFactor.SrcAlpha:
//					_src = BlendingFactorSrc.SrcAlpha;
//					break;
//					 
//				case BlendFuncFactor.OneMinusSrcAlpha:
//					_src = BlendingFactorSrc.OneMinusSrcAlpha;
//				  	break;
//
//				case BlendFuncFactor.One:
//					_src = BlendingFactorSrc.One;
//				  	break;
//				  	
//				case BlendFuncFactor.DstColor:
//					_src = BlendingFactorSrc.DstColor;
//				  	break;				  	
//				  	
//				default:
//					Debug.Assert(false);
//					break;
//			}
//			switch (dstFactor)
//			{
//				case BlendFuncFactor.Zero:
//					_dst = BlendingFactorDest.Zero;
//					break;
//					
//				case BlendFuncFactor.SrcAlpha:
//					_dst = BlendingFactorDest.SrcAlpha;
//					break;
//					 
//				case BlendFuncFactor.OneMinusSrcAlpha:
//					_dst = BlendingFactorDest.OneMinusSrcAlpha;
//				  	break;
//				  	
//				case BlendFuncFactor.One:
//					_dst = BlendingFactorDest.One;
//				  	break;
//				  	
//				case BlendFuncFactor.DstColor:
//					_dst = BlendingFactorDest.DstColor;
//				  	break;	
//				  	
//				default:
//					Debug.Assert(false);
//					break;
//			}
//			//FIXME:??????in SpriteSample, for sprite black bg problem
//			//GL.DepthMask(false); //not good
//			GL.Clear(ClearBufferMask.DepthBufferBit);
//			GL.BlendEquation(_mode);
//			GL.BlendFunc(_src, _dst);
//		}
//		
//		public void __setblend()
//		{
//	        this.Enable(EnableMode.Blend);
//	        this.SetBlendFunc(BlendFuncMode.Add, BlendFuncFactor.SrcAlpha, BlendFuncFactor.OneMinusSrcAlpha);
//		}
		
		public void SetFrameBuffer (FrameBuffer buffer)
		{
			//Debug.Assert(false);
			if (buffer != null)
			{
				Debug.Assert(buffer.__framebufferId >= 0);
				this.__frameBuffer = buffer;
				GL.BindFramebuffer(FramebufferTarget.Framebuffer, buffer.__framebufferId);
			}
			else
			{
//				Debug.WriteLine("=================buffer == null");
//				this.__frameBuffer = buffer;
//				GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);		
				this.__frameBuffer = this.__screen;
				GL.BindFramebuffer(FramebufferTarget.Framebuffer, this.__screen.__framebufferId);
			}
		}
		
//		public void SetCullFace (CullFaceMode mode, CullFaceDirection direction)
//		{
//			OpenTK.Graphics.ES20.CullFaceMode mode_ = 0;
//			OpenTK.Graphics.ES20.FrontFaceDirection mode2_ = 0;
//			switch (mode)
//			{
//				case CullFaceMode.Back:
//					mode_ = OpenTK.Graphics.ES20.CullFaceMode.Back;
//					break;
//			}
//			switch (direction)
//			{
//				case CullFaceDirection.Ccw:
//					mode2_ = OpenTK.Graphics.ES20.FrontFaceDirection.Ccw;	
//					break;
//			}
//			GL.CullFace(mode_);
//			GL.FrontFace(mode2_);
//			//Debug.Assert(false);
//			__curCullFace = new CullFace(mode, direction);
//		}
		
//		public bool IsEnabled(EnableMode mode)
//		{
//			EnableCap mode_ = 0; //FIXME:
//			switch (mode)
//			{
//				case EnableMode.CullFace:
//					mode_ = EnableCap.CullFace;
//					break;
//					
//				case EnableMode.DepthTest:
//					mode_ = EnableCap.DepthTest;
//					break;
//					
//				case EnableMode.Blend:
//					mode_ = EnableCap.Blend;
//					break;
//				
//				case EnableMode.ScissorTest:
//					mode_ = EnableCap.ScissorTest;
//					break;
//					
//				case EnableMode.StencilTest:
//					mode_ = EnableCap.StencilTest;
//					break;
//					
//				default:
//					Debug.Assert(false);
//					break;
//			}
//			return GL.IsEnabled(mode_);
//		}
		
		public void SetLineWidth(float width)
		{
			//Debug.Assert(false);
			GL.LineWidth(width);
		}
		
//		public ImageRect GetViewport()
//		{
//			//Debug.Assert(false);
////			Debug.WriteLine("======================>GetViewport not implemented");
//			float winW = SakuraGameWindow.getWidth();
//			float winH = SakuraGameWindow.getHeight();
//			return new ImageRect(0, 0, (int)winW, (int)winH);
//		}
		
//		public void Disable(EnableMode mode)
//		{
////			Debug.Assert(false);
//			Enable(mode, false);
//		}
		
//		private BlendFunc __curBlendFunc;
//		public void SetBlendFunc(BlendFunc func)
//		{
//			//Debug.Assert(false);
////			Debug.WriteLine("======================>SetBlendFunc not implemented");
////			if (func != null)
////			{
////				this.SetBlendFunc(func.mode, func.srcFactor, func.dstFactor);
////			}
////			else
////			{
////				Debug.Assert(false);
////			}
//			this.SetBlendFunc(func.mode, func.srcFactor, func.dstFactor);
//			__curBlendFunc = func;
//		}
		
//		private DepthFunc __depthFunc = new DepthFunc();
//		public DepthFunc GetDepthFunc()
//		{
//			//FIXME:
//			//Debug.Assert(false);
//			return __depthFunc;
//		}
//		
//		public void SetDepthFunc(DepthFunc func)
//		{
//			//FIXME:
//			//Debug.Assert(false);
//			__depthFunc = func;
//			GL.DepthFunc(func.Mode);
//		}
		
//		public BlendFunc GetBlendFunc()
//		{
//			//Debug.Assert(false);
//			//return null;
//			return __blendFunc;
//		}
		
//		private ImageRect __scissorRect = new ImageRect(0, 0, 0, 0); //FIXME:???
//		public ImageRect GetScissor() 
//		{
////			Debug.Assert(false);
//			return __scissorRect;
//		}
		
//		public void SetScissor(int x, int y, int w, int h)
//		{
////			Debug.Assert(false);
//			__scissorRect = new ImageRect(x, y, w, h);
//			GL.Scissor(x, y, w, h);
//		}
		
//		public void ReadPixels(byte[] pixels, PixelFormat format, int sx, int sy, int sw, int sh)
//		{
////			Debug.Assert(false);
//			if (format == PixelFormat.Rgba)
//			{
//				GL.ReadPixels<byte>(sx, sy, sw, sh, OpenTK.Graphics.ES20.PixelFormat.Rgba, PixelType.Byte, pixels);
//			}
//			else
//			{
//				Debug.Assert(false);
//			}
//		}
		
//		public void SetScissor(ImageRect rectangle)
//		{
////			Debug.Assert(false);
//			this.SetScissor(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
//		}
//		
//		public void SetViewport(ImageRect rectangle)
//		{
////			Debug.Assert(false);
//			this.SetViewport(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
//		}
		
//		public void SetBlendFuncAlpha(BlendFuncMode arg1, BlendFuncFactor arg2, BlendFuncFactor arg3)
//		{
////			Debug.Assert(false);
//			SetBlendFuncAlpha(new BlendFunc(arg1, arg2, arg3));
//		}
//		
//		public void SetBlendFuncRgb(BlendFuncMode arg1, BlendFuncFactor arg2, BlendFuncFactor arg3)
//		{
////			Debug.Assert(false);
//			SetBlendFuncRgb(new BlendFunc(arg1, arg2, arg3));
//		}
		
//		public GraphicsCaps Caps
//		{
//			get
//			{
////				Debug.Assert(false);
//				GraphicsCaps caps = new GraphicsCaps();
//				//GL_MAX_TEXTURE_SIZE 
//				caps.MaxTextureSize = GL.GetInteger(GetPName.MaxTextureSize);
//				return caps;
//			}
//		}
		
//		private BlendFunc __curBlendFuncRgb;
//		public BlendFunc GetBlendFuncRgb()
//		{
////			Debug.Assert(false);
////			return null;
//			return __curBlendFuncRgb;
//		}
//		public void SetBlendFuncRgb(BlendFunc v)
//		{
////			Debug.Assert(false);
//			__curBlendFuncRgb = v;
//		}
//		private BlendFunc __curBlendFuncAlpha;
//		public BlendFunc GetBlendFuncAlpha()
//		{
////			Debug.Assert(false);
////			return null;
//			return __curBlendFuncAlpha;
//		}
//		public void SetBlendFuncAlpha(BlendFunc v)
//		{
////			Debug.Assert(false);
//			__curBlendFuncAlpha = v;
//		}
		
		public Vector4 GetClearColor()
		{
//			Debug.Assert(false);
//			return new Vector4();
			return __curClearColor;
		}
		
//		private ColorMask __curColorMask;
//		public ColorMask GetColorMask()
//		{
////			Debug.Assert(false);
////			return ColorMask.A;
//			return __curColorMask;
//		}
//		public void SetColorMask(ColorMask v)
//		{
////			Debug.Assert(false);
//			GL.ColorMask(
//				(v & ColorMask.R)!=0,
//				(v & ColorMask.G)!=0,
//				(v & ColorMask.B)!=0,
//				(v & ColorMask.A)!=0);
//			__curColorMask = v;
//		}
		
//		private CullFace __curCullFace;
//		public CullFace GetCullFace()
//		{
////			Debug.Assert(false);
////			return new CullFace();
//			return __curCullFace;
//		}
//		public void SetCullFace(CullFace v)
//		{
////			Debug.Assert(false);
//			SetCullFace(v.Mode, v.Direction);
//			__curCullFace = v;
//		}
		public ShaderProgram GetShaderProgram()
		{
//			Debug.Assert(false);
//			return null;
			return __curProgram;
		}
	}
}
