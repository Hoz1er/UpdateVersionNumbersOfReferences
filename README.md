<script type="text/javascript" src="http://flowchart.js.org/flowchart-latest.js"></script>
# UpdateWebCiteVersion
Update(/Add) Cite Version by file's hash code ( C#  Console )

<p style="color:blueviolet;font-weight:900;font-size:34px;">1.使用方式：</p>
<p style="color:black;font-size:22px;text-indent:60px;">输入项目根目录的路径</p>
<p style="color:blueviolet;font-weight:900;font-size:34px;">2.使用示例：</p>

![实例截图](https://github.com/hgx-Obsess10n/UpdateWebCiteVersion/blob/master/example/example_0.png?raw=true)

<a style="margin-left:20px;text-decoration:underline;" href="https://github.com/hgx-Obsess10n/WebSLN">点此链接打开上面例子所用项目</a>
<p style="color:black;font-size:22px;text-indent:60px;"></p>

<p style="color:blueviolet;font-weight:900;font-size:34px;">3.流程图：</p>
<p style="color:blueviolet;font-weight:600;font-size:22px;">(1)流程图1[主流程]：</p>

```flow

st=>start: Start
e=>end: End
io1=>inputoutput: 输入项目路径
op1=>operation: 生成备份目录、日志目录
op2=>operation: 获取当前目录下文件数组FileInfo[], i=0
c1=>condition: i &lt; FileInfo.length?
c2=>condition: FileInfo[i]需要处理?
op3=>operation: i++
op4=>operation: 处理该文件（流程图2）
op5=>operation: 获取目录下子目录数组DirectoryInfo[], k=0
c3=>condition: k &lt; DirectoryInfo.length?
op6=>operation: 进入该目录

st->io1->op1->op2
op2(bottom)->c1
c1(yes)->c2
c2(no,bottom)->op3
c2(yes)->op4
op4->op3
op3(left)->c1
c1(no)->op5
op5->c3
c3(yes,right)->op6(right)->op2
c3(no)->e
```
<p style="color:blueviolet;font-weight:600;font-size:22px;">(2)流程图2[单个文件更新]：</p>

```flow

st=>start: Start
e=>end: End
op1=>operation: 生成备份文件和临时文件
op2=>operation: 以临时文件替换旧文件，并删除临时文件
c1=>condition: StreamReader.EndOfStream?
op3=>operation: line=StreamReader.ReadLine()
c2=>condition: line中含有引用信息?
op4=>operation: 分析并修改line(流程图3)
op5=>operation:  line写入临时文件

st->op1->c1
c1(yes)->op2->e
c1(no)->op3->c2
c2(no,left)->op5
c2(yes,bottom)->op4(right)->op5
op5(right)->c1
```

<p style="color:blueviolet;font-weight:600;font-size:22px;">(3)流程图3[单行字符串更新]：</p>

```flow

st=>start: Start
e=>end: End
op1=>operation: 截取引用文件字符串
op2=>operation: 分析引用路径，获取文件的完整路径
c1=>condition: 字典中有该文件的hash值?
op3=>operation: 将该文件的完整路径及hash值写入字典
op4=>operation: 在字典中取出该文件的hash值
op5=>operation: 分析文件引用后面所附带的参数，并加入/更新版本号（hash值）
op6=>operation: 将分析时拆解得到的各部分信息重新拼接为一行字符串

st->op1->op2->c1
c1(yes)->op4->op5->op6->e
c1(no)->op3->op4
```

