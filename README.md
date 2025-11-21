# GameModule

## 通用组件
- `NavigationIndicator` 方向导航器
- `Mirror` 对称UI自动镜像对齐 (与 `MirrorEditor` 编辑器脚本配合使用)
- `UIPolygon` 在 uGUI 上绘制图形
- `EmptyRaycast` uGUI 空白事件检测组件
- `MainThreadDispatcher` 主线程事件分发，处理 Unity 多线程问题
- `UIFullScreenAdapter` 忽略安全区域，全屏适配组件

## Editor 工具
- `NormalsVisualizer` 模型法线可视化
- `ExportTerrain` 将 Unity 地形导出成 obj 格式的模型
- `ObjExporter` 将 Mesh 导出成 obj 格式模型

## 工具类
- `RectTransformUtility` RectTransform 工具类
- `TimeUtility` 时间工具类

## 其他模块
- `ObjectPool` 对象池
- `EventDispatcher` 事件分发
- `Singleton` 单例模式
- `IndicatorSystem` 屏幕外对象指示器系统，用来显示屏幕外对象的位置和方向
