# sakura2
Sakura Mk2, Sakura is PSS/PSM SDK reverse engineering with OpenTK, Mono, dotNet and etc

## Ref  
* https://github.com/weimingtom/Sakura
* https://gitee.com/weimingtom/sakura_ubuntu
* https://sourceforge.net/projects/opentk/files/opentk/opentk-1.1/stable-5/  

## History  
* 2024-12-28: Restart project, running simple hello successfully under ubuntu without ANGLE so files. (libEGL.so and libGLESv2.so removed, see sakura_ubuntu libGLESv2angle.so)    
* From about 2018-01-15 to 2018-03-22: Develop it for Windows XP with ANGLE dlls and SharpDevelop 4.4, but porting to Linux difficultly, too many bugs to fix.        

## Weibo record before   
```
我已经把可以运行最简单例子的PSSSDK的OpenTK移植版迁移到gh上，
名字叫Sakura，以后可能大部分的3d学习都会基于这个东西。。。

Sakura的问题，下午没留意，原来win7和xp下ANGLE的行为不一样，
在XP下ANGLE只接受2的n次幂的长宽纹理，
所以在生成纹理的时候要预留空白的位置。。。

Sakura记录，移植第2个例子SpriteSample成功。
之前的纹理色彩rgb搞反了，
不过在移植TriangleSample的时候没发现。。。 

Sakura进度，现在移植成功第3个demo，
这个demo主要是点线面的绘画，难度很小 ​​​

Sakura移植，这次是MathSample，不是很难，因为可以复制反编译代码。。。
发现Csharp可以重定向Console类的输出，试下的确是可行的，
但不如pss的模拟器输出得快。。。

Sakura移植，移植PixelBufferSample成功，这个例子要棘手一些，
它演示了怎样画一个立方体（需要用到glDrawElements和索引数组），
更麻烦的是FBO（framebuffer），它用FBO把三角形RGB例子画到一个离屏纹理上，
然后把这个纹理贴到立方体的六个面上。。。

感觉sakura再移植下去就变成一个完全不面向对象，反人类的全局状态机了。。。
PSSSDK的写法是可以随意调换执行的位置而不改变显示效果，
而OpenGL ES调换执行次序就有问题了，所以实现起来非常别扭。。。

Sakura移植，PSSSDK的Graphics最后一个例子ShaderCatalogSample移植大致完成了，
比较困难的是怎样把好几个着色器cg代码转换成glsl代码（有很多），
方法是通过cgc命令行转换成glslv和glslf格式，
然后用PSSSDK安装目录下的一个python脚本转换成正常的着色器，
不过我发现还需要修改一些地方，例如一些纹理uniform变量要改成类似Texture0的写法。
剩下的工作是改掉一些bug，主要是opengles一些需要按次序执行的操作，
如果要正常移植到PSSSDK的写法需要做一些手脚。。。

Sakura移植，Graphics最后的例子ShaderCatalogSample可以在XP下正常运行了，
昨天的NPOT问题解决了。因为XP下ANGLE不支持NPOT，
只能通过拉伸的方式把纹理转成POT尺寸，代价是纹理模糊了（显示字体也模糊了）。
不过只在使用repeat的wrap模式下才需要转换，平时是可以不转的
（依靠GL_CLAMP_TO_EDGE的力量），我在代码中写死一定要转成POT，
是方便我测试POT的转换效果（我的恶趣味）。。。

Sakura移植，Audio的第一个例子BgmPlayerSample移植成功，
我是用gh上一个最常见的方法OpenTK里面的OpenAL接口和一个开源音频解码库
NAudio实现mp3文件播放（话说真有基于OpenTK的游戏引擎，不过OpenTK的dll大小有点吓人）。
另一个问题是同一深度的矩形深度不对，例如下方那个白色和红色重叠的矩形，
默认会把红色的遮住，我的解决方法是在初始化GLES时设置深度函数
glDepthFunc(GL_LEQUAL);
glHint(GL_PERSPECTIVE_CORRECTION_HINT, GL_NICEST);
参考这里：
http://tiankefeng0520.iteye.com/blog/2008008  
并且在GLES初始化完成后立刻执行glClear和SwapBuffers，
这样的话即使其他初始化耗费的时间很长，弹出的窗口仍会显示黑色底色

Sakura移植，移植Audio第二个也是最后一个例子SoundPlayerSample成功，
这个跟BgmPlayerSample几乎一样，不同的是这个例子的声音可以连续
（就是说还没播放完就按播放的话会从头再播放一次），
类似子弹发射的音效。。。

Sakura移植，移植Input第一个例子GamePadSample成功，
右边那一排是手柄杆analog stick的参数，没有实现。
左边的按钮都实现了。字体调成清晰，是因为我强制使用NPOT避免拉伸模糊。
原来的代码有个bug，每次渲染的时候都要执行SampleDraw.ClearSprite();清除
所有文本精灵，导致不断地创建纹理。在我的实现里会内存过大溢出，
而在PSS的模拟器里面运行也会有很大概率崩溃报错，
原因不明，所以最后我还是把这句代码注释掉。。。

Sakura移植，Input第二个例子MotionSample完成，这个跟上一个类似，
不过因为Motion动作数据没办法模拟，所以先搁着。。。
这个例子同样清除了文本纹理缓存，我把它去掉了

Sakura移植，第三个例子TouchSample完成，这个例子没办法模拟多点触碰，
所以只能看到一种颜色。。。

Sakura移植，Imaging的两个例子FontSample, ImageSample完成，
前者因为之前已经有过字体移植的基础，所以很快就搞定了，
后者需要扩展Image类的方法，我用Bitmap和Graphics类模拟图片
的加载、裁剪、缩放等操作。。。 

Sakura移植，Network的两个例子HttpSample、SocketSample完成，
这两个例子没有涉及新的API，都是csharp的内置网络库，
所以难度不大，不过后者有点问题，我擅自修改了SampleButton的实现，
使其在按钮文本不变的情况下SetText()方法不会生成新的纹理，
避免纹理不断被创建。。

Sakura移植，ClipboardSample完成，这个的难点是做一个类似vb的
InputDialog。。。网上有参考代码，只要改一下就好

Sakura移植，现在Environment的七个例子全部完成，除了之前说的
ClipboardSample和DialogSample以外，还有ShellSample是打开外部浏览器，
SystemEventsSample是最小化恢复到正常的事件响应，
SystemParametersSample是系统参数（我返回常量），
StorageSample是写入任意文本（可指定文件名），
在程序下次运行时数据仍然可读，而PersistentMemorySample是
写入二进制内存，最大64KB，我实现成二进制文件读写，
读取的时候总是返回64KB的字节数组

最近Sakura已经大致移植完core部分的API和例子，
接下来的例子是基于GameEngine2D，
在命名空间Sce.Pss.HighLevel下，
而这个包里面的API迷之类似cocos2d-x，例如RunWithScene。。。

可能这几个星期都没时间研究sakura移植和scutserver的代码。。。
虽然我是有点赌气，但有时候还是要面向残酷现实，专心赚钱，
其实工作并不比研究开源差，因为做人一定要相信明天会更好 ​​​

Sakura移植，因为我买了台PSP，所以我又有斗志去移植PSSSDK了，
然后今天终于解决了卡住2个月的疑难问题：
在移植GameEngine2D包HelloSprite例子时，
我发现纹理被莫名其妙地放大了1.333倍，困扰了很久，
最终发现其实这个bug不是因为OpenGL ES的问题，
而是因为我在获取内存位图时画错了，正确的写法应该是
System.Drawing.Graphics.DrawImage不能只是指定画在(0,0)坐标上，
还需要额外传递目标矩形的长宽，否则会被放大1.333倍。
改好这个bug后显示出来就是正常的纹理了

Sakura移植，GameEngine2D的FeatureCatalog基本完成，
除了一些细节问题和BlendMode.None无法把透明色替换成白色底色外（太难处理，放弃了），
不过基本效果都实现了，以后再慢慢改bug。这个例子是一个集成测试，
但仍旧不是一个完整的游戏 ​​​

Sakura移植，GameEngine2D下的ActionGameDemo例子移植成功，
除了ColorBuffer未实现，其他都很轻松。这是第一个完整的游戏例子，
类似横版格斗游戏，不过活动范围是有限的
```

## Weibo record after    
```
你可能会问，跨平台运行dotNet程序，除了mono和dotgnu，
不是还有个dotnet core吗？这个问题比较难，
我是只想研究比较简单的，例如JVM我只研究一两个，
OpenJDK是懒得研究。dotnet core的情况有点像openjdk，
虽然功能会比较全比较新，但我还是喜欢mono，
毕竟我用mono的时间更长——主要是用于我的Sakura项目，
把PSS SDK逆向工程然后移植到mono上，
在Linux下跑ANGLE模拟OpenGLES

用ubuntu（至少需要22，我用的是xubuntu 22.04）和
dotnetcore（sudo apt install dotnet-sdk-8.0）
也可能很方便地编译运行OpenTK程序，调用OpenGL接口——当然有个前提，
必须能获取到OpenTK的dll。似乎只有glfw那里是区分平台的。
我想研究嵌入式设备跑mono或dotnetcore环境的OpenGLES基于OpenTK或类似库，
当然这只是初步的想法。其实理论上是可以用树莓派跑OpenTK的，
不过我的研究目标是不需要X11环境的OpenTK，
所以可能需要更深度修改OpenTK源代码
（我以前的项目Sakura就是想做到这个目标，不过没完整做出来，
我可能另外创建一个新的项目去研究这个问题）

我觉得OpenTK只有古老的1.1版可以在linux下工作。
我试过OpenTK 3.x在xubuntu 20下不好使，但在windows下是正常的。
如果opentk 4则是给dotnet core用的了，我没测试过。
我以前研究的是opentk 1.1，代码在sf上，
虽然看上去很古老（10年前）不过却可以用linux版mono正常运行
（虽然有些地方如opengles效果是不对的），
我就是基于这个版本来开发Sakura的，
我打算重启这个游戏引擎项目，达到类似godot和unity的作用

我以前的Sakura项目（基于逆向工程反编译的PSS SDK）在
xubuntu 20下用monodevelop运行的效果，我想通了，
可以不需要编译angle库，直接用ubuntu自带的libEGL和libGLESv2运行即可
（它也是靠mesa模拟出来的）。实践证明这和angle的效果是一样的，
也可以正常显示。只不过这只是X11的效果，类似于GLFW3，
但如果要移植到Linux掌机，需要去掉X11的相关代码
（Linux掌机上没有x11环境，但可以运行EGL和GLES2代码）。
我需要研究，如果OpenTK没有简答的方法的话，只能自己动手写代码实现了
```
