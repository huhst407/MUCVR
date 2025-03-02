# MUCVR

## 文件目录

```text
MUCVR/
├── KCPCloudClient/ # 边缘端程序
├── KCPCloudServer/  #云端程序
├── .gitignore/
├── Readme.md/
```



## 使用环境

### 硬件环境

云端

云服务器

边缘端

推荐使用GPU Nvidia GeForce RTX3060, CPU i5-9300 

移动端

oculus quest2

### 软件环境

边缘端虚拟出来的主机都需要安装steamVR和ALVR

移动端需要安装ALVR

## 部署

KCPCloudClient，KCPCloudServer导入相应的场景资源包

将需要的场景另存为新的一份场景。

新建一个GameObject修改名称为NetworkManager。KCPCloudServer挂载CENetworkManager修改IP地址和端口。

KCPCloudClient挂载CENetworkManager修改IP地址和端口，虽然命名一样但脚本内容不同需多加注意。将NetworkManager和NetworkManagerHUD也挂载到命名为NetworkManager的GameObject上。

将边缘端的移动的场景物体添加NetworkTransform，将边缘端所有物体上的MeshRender的enable设为false，云端的MeshRender的enable为true。

## 使用

完成后打包程序后分别部署到云端，边缘端。

启动移动端和边缘端的ALVR

运行程序。
