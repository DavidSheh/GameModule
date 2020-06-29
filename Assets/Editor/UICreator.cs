using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine.UI;
using System;

public class UICreator : EditorWindow
{
    private string luaFolder = string.Empty;
    private string moduleName = string.Empty;
    private string className = string.Empty;
    private string luaRootPath = string.Empty;
    private string resPath = string.Empty;

    private const string ResPathTips = "在 respath_cfg.lua 脚本中添加 UI 的路径和 ID,并把 ID 填到此处";
    private const string strModule = "Module";

    [MenuItem("UI工具/AUTO_GEN_LUA_CODE")]
    [MenuItem("GameObject/UI/AUTO_GEN_LUA_CODE  &q", false, 0)]
    public static void OpenEditor()
    {
        var winds = (UICreator)GetWindow(typeof(UICreator), true, "Gen Lua Code");
        winds.minSize = new Vector2(300, 400);
        winds.luaRootPath = Application.dataPath.Replace("UnityPrj/Assets", "Output/Lua/Game");
    }

    private GameObject currentSelect;

    public void OnGUI()
    {
        if (Selection.activeGameObject != currentSelect)
        {
            ChangeProperty();
            Init(currentSelect.transform);
        }

        EditorGUILayout.BeginVertical();
        GUILayout.Space(20);
        if (currentSelect != null)
        {
            GUILayout.BeginHorizontal();
            luaRootPath = EditorGUILayout.TextField("Code Path:", luaRootPath);
            if (GUILayout.Button("Select", GUILayout.Width(100)))
            {
                luaRootPath = EditorUtility.SaveFolderPanel("Selet Code Path", Path.Combine(Application.dataPath, "CSharpScripts/UUI/Windows"), "");
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            luaFolder = EditorGUILayout.TextField("LuaFloder:", luaFolder);
            if (GUILayout.Button("Refresh", GUILayout.Width(100)))
            {
                ChangeProperty(luaFolder);
            }
            GUILayout.EndHorizontal();
            className = EditorGUILayout.TextField("ClassName:", className + ".lua");

            GUILayout.BeginHorizontal();
            if (!string.IsNullOrEmpty(moduleName))
            {
                moduleName += ".lua";
            }
            moduleName = EditorGUILayout.TextField("ModuleName:", moduleName);
            EditorGUILayout.LabelField("注意：ModuleName 为空表示不生成 Module 类的脚本和 NotifyId.lua");
            GUILayout.EndHorizontal();

            resPath = EditorGUILayout.TextField("ResPath:", resPath);

            className = className.Replace(".lua", "");
            moduleName = moduleName.Replace(".lua", "");

            if (showExample = EditorGUILayout.Foldout(showExample, "导出的对象信息列表"))
            {
                scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(200));
                EditorGUILayout.BeginVertical();
                foreach (var i in infoList)
                {
                    GUIStyle style = new GUIStyle
                    {
                        richText = true
                    };
                    GUILayout.Label("<color=green>Name:</color> <color=white>" + i.Name + ", </color>" +
                        "<color=green>Path:</color> <color=white>" + i.Path + ", </color>" +
                        "<color=white>Type:</color> <color=green>" + i.Type + "</color>", style);
                }

                EditorGUILayout.EndVertical();
                GUILayout.EndScrollView();
            }

            var rect = new Rect(position.width - 105, position.height - 25, 100, 20);
            if (GUI.Button(rect, "Gen"))
            {
                string warning = string.Empty;
                if (string.IsNullOrEmpty(resPath) || string.Equals(resPath, ResPathTips))
                {
                    warning = "ResPath 项为空！需要在 respath_cfg.lua 脚本中添加一对键值，然后将其中的整形类型的键填入 ResPath 项中。如果不在这里设置 ResPath 的值，可稍后在代码里设置。";
                    if (EditorUtility.DisplayDialog("警告", warning, "Create", "Cancel"))
                    {
                        Export();
                    }
                }
                else
                {
                    Export();
                }
            }
        }
        EditorGUILayout.EndVertical();
    }

    void ChangeProperty(string folder = null)
    {
        currentSelect = Selection.activeGameObject;
        string name = currentSelect.name;
        if (name.Length > 3)
        {
            string namePrefix = name.Substring(0, 3);// UIXxx, 前面有三个大写字母
            var strClassName = namePrefix.ToUpper() + name.Substring(3, name.Length - 3);
            var arrayNames = strClassName.Split('_');
            className = string.Empty;
            foreach (var a in arrayNames)
            {
                if(a.Length < 1)
                {
                    continue;
                }
                className += a.Substring(0, 1).ToUpper() + a.Substring(1, a.Length - 1);
            }
        }
        if (className.Length > 2)
        {
            if (string.IsNullOrEmpty(folder))
            {
                string firstStr = name.Split('_')[0];
                firstStr = firstStr.Substring(2, firstStr.Length - 2);// 去掉 UI 两个字符
                luaFolder = firstStr.Substring(0, 1).ToUpper() + firstStr.Substring(1, firstStr.Length - 1); // 将第一个字母大写
            }
            else
            {
                luaFolder = folder;
            }
            moduleName = luaFolder + strModule;
        }

        scriptPath = null;
        scriptContent = null;
        resPath = GetResPath();
    }

    private const string uiLuaScript =
    @"local Lplus = require('Lplus')
local UIPanelBase = require('Game.GUI.UIPanelBase')
local CLASSNAME = Lplus.Extend(UIPanelBase,'CLASSNAME')
local GameUtil = require('Main.GameUtil')
local def = CLASSNAME.define
local m_Instance = nil
[UIVARS]

def.static('=>',CLASSNAME).Instance = function()
    if(m_Instance == nil) then
        m_Instance = CLASSNAME()
    end
    return m_Instance
end

--创建UI
def.override().DoCreate = function(self)
    print('CLASSNAME: DoCreate ------------')
    self:CreateUGUIPanel(RESPATH), UILevel.HideSame, {})
end

--创建完成
def.override().OnCreate = function(self)
    print('CLASSNAME: OnCreate ------------')
    [GETUI]
end

--显示回调
def.override('boolean').OnShow = function(self,show)
    if(show) then
        print('CLASSNAME: OnShow ------------')
    end
end

--显示后回调
def.override().AfterCreate = function(self)
    print('CLASSNAME: AfterCreate ------------')
    -- Event.RegisterMemberEvent(ModuleId.Settlement, gmodule.notifyId.Settlement.AGREE_OTHER, m_Instance, UISettlement.OnAgreeOther);
end

-- 单击事件回调
def.method('userdata').onClickObj = function (self, obj)
    print('CLASSNAME: onClickObj ------------' .. obj.name);
    if (obj.name == self.btnClose.name) then
        self:BackToLastPanel();
    end
end

--销毁回调
def.override().OnDestroy = function(self)
    print('CLASSNAME: OnDestroy ------------')
    -- Event.UnRegisterEvent(ModuleId.Settlement, gmodule.notifyId.Settlement.AGREE_OTHER, UISettlement.OnAgreeOther);
end

--刷新界面
def.method().UpdateUI = function(self)
    print('CLASSNAME: UpdateUI ------------')
end

CLASSNAME.Commit()
return CLASSNAME";

    private const string moduleLuaScript =
    @"local Lplus = require('Lplus')
local ModuleBase = require('Main.Module.ModuleBase')
local MODULENAME = Lplus.Extend(ModuleBase,'MODULENAME')
require('Main.Module.ModuleId')
local GameNet = require('Net.GameNet')
local def = MODULENAME.define
local m_Instance = nil

def.static('=>',MODULENAME).Instance = function()
    if(m_Instance == nil) then
        m_Instance = MODULENAME()
        m_Instance.m_moduleId = ModuleId.MODULEID
    end
    return m_Instance
end

def.override().Init = function(self)
    ModuleBase.Init(self)
    -- GameNet.AddResponse(ProtoRecive.onUseItem,MODULENAME.onUseItemRev)
end

--==============request=======================
--发送道具使用
-- def.method('number','number').SendUseItem = function(self,itemid,count)
--     GameNet.SendRequest(ProtoSend.useItem,{itemid,count})
--     print('发送背包使用消息 Item id = '..itemid..' Count = '..count)
-- end

--==============Response=======================
-- def.static('table').onUseItemRev = function(params)
--     print('收到背包道具使用成功消息')
--     printObject(params)
--     Event.DispatchEvent(ModuleId.Bag,gmodule.notifyId.Bag.BAG_USE_SUC, params[2])
-- end

MODULENAME.Commit()
return MODULENAME";

    private const string notifyLuaScript =
        @"local NotifyId = 
{
    -- TODO: write notify id. eg：BAG_USE_SUCC = 1, --背包使用道具成功
}
return NotifyId";

    private const string fieldRegion = "--#region Field\n--自动生成的代码，请勿在此代码区域内修改或添加其他代码{0}\n--#endregion Field";
    private const string fieldDefine = "\ndef.field('userdata').m{0} = nil;";
    private const string findRegion = "--#region Find\n\t--自动生成的代码，请勿在此代码区域内修改或添加其他代码{0}\n\t--#endregion Find";
    private const string fieldFind = "\n\tself.m{0} = self:FindChild('{1}')";
    private const string compGet = ":GetComponent('{0}')";

    private void Export()
    {
        var fields = new StringBuilder();
        var fieldFinds = new StringBuilder();
        foreach (var item in infoList)
        {
            fields.Append(string.Format(fieldDefine, item.Name));

            fieldFinds.Append(string.Format(fieldFind, item.Name, item.Path));
            if (!string.IsNullOrEmpty(item.Type))
            {
                if (item.Type.Equals("GameObject"))
                {
                    fieldFinds.Append(".gameObject");
                }
                else if (!item.Type.Equals("Transform"))
                {
                    fieldFinds.Append(string.Format(compGet, item.Type));
                }
            }
            fieldFinds.Append(";");
        }

        string scriptPath = GetScriptPath();
        string uiStr = GetScriptContent();
        if (!string.IsNullOrEmpty(uiStr))
        {
            bool canUpdate = EditorUtility.DisplayDialog("", string.Format("{0}.lua 脚本已存在，是否更新自动生成的代码？", className), "确定", "取消");
            if(canUpdate)
            {
                int startIdx = uiStr.IndexOf("--#region Field");
                int endIdx = uiStr.IndexOf("--#endregion Field") + "--#endregion Field".Length;
                string str = uiStr.Substring(startIdx, endIdx - startIdx);
                string replace = string.Format(fieldRegion, fields.ToString());
                uiStr = uiStr.Replace(str, replace);

                startIdx = uiStr.IndexOf("--#region Find");
                endIdx = uiStr.IndexOf("--#endregion Find") + "--#endregion Find".Length;
                str = uiStr.Substring(startIdx, endIdx - startIdx);
                replace = string.Format(findRegion, fieldFinds.ToString());
                uiStr = uiStr.Replace(str, replace);

                uiStr = uiStr.Replace("'", "\"");
                File.WriteAllText(scriptPath, uiStr);
                EditorUtility.DisplayDialog("", "Lua 生成成功了，快去看看吧！", "确定");
            }
        }
        else
        {
            uiStr = uiLuaScript.Replace("CLASSNAME", className);
            uiStr = uiStr.Replace("RESPATH", resPath);
            string strField = string.Format(fieldRegion, fields.ToString());
            uiStr = uiStr.Replace("[UIVARS]", strField);
            uiStr = uiStr.Replace("[GETUI]", string.Format(findRegion, fieldFinds.ToString()));
            uiStr = uiStr.Replace("'", "\"");

            if (!(string.IsNullOrEmpty(moduleName) || string.IsNullOrEmpty(moduleName.Trim())))
            {
                string moduleStr = moduleLuaScript.Replace("MODULENAME", moduleName);
                string moduleId = moduleName;
                moduleId.Replace("Module", "");
                moduleLuaScript.Replace("MODULEID", moduleId);
                string path = Path.Combine(luaRootPath, luaFolder);
                moduleStr = moduleStr.Replace("'", "\"");
                File.WriteAllText(string.Format("{0}/{1}.lua", path, moduleName), moduleStr);

                File.WriteAllText(string.Format("{0}/NotifyId.lua", path, moduleName), notifyLuaScript);
            }
            uiStr = uiStr.Replace("'", "\"");
            File.WriteAllText(scriptPath, uiStr);
            EditorUtility.DisplayDialog("", "Lua 生成成功了，快去看看吧！", "确定");
        }
    }

    private static string scriptContent = null;
    public string GetScriptContent()
    {
        if (string.IsNullOrEmpty(scriptContent))
        {
            string scriptPath = GetScriptPath();
            if (File.Exists(scriptPath))
            {
                scriptContent = File.ReadAllText(scriptPath);
            }
        }
        return scriptContent;
    }

    private static string scriptPath = null;
    private string GetScriptPath()
    {
        if (string.IsNullOrEmpty(scriptPath))
        {
            string path = Path.Combine(luaRootPath, luaFolder);
            string uiLuaPath = path + "/ui";
            if (!Directory.Exists(uiLuaPath))
            {
                Directory.CreateDirectory(uiLuaPath);
            }
            scriptPath = string.Format("{0}/{1}.lua", uiLuaPath, className);
        }

        return scriptPath;
    }

    private string GetResPath()
    {
        var script = GetScriptContent();
        if(string.IsNullOrEmpty(script))
        {
            return ResPathTips;
        }

        string targetStr = "self:CreateUGUIPanel(GameUtil.GetResPath(";
        int index = script.IndexOf(targetStr);
        if(index > 0)
        {
            index += targetStr.Length;
            int endIndex = script.IndexOf("),");
            int len = endIndex - index;
            if (len < 20)
            {
                string resPathId = script.Substring(index, len);
                return resPathId;
            }
        }

        return ResPathTips;
    }

    private Vector2 scroll;
    private bool showExample = true;

    private static Dictionary<Type, string> types = new Dictionary<Type, string>{
        { typeof(GridLayoutGroup), "GridLayoutGroup" },
        { typeof(VerticalLayoutGroup), "VerticalLayoutGroup" },
        { typeof(HorizontalLayoutGroup), "HorizontalLayoutGroup" },
        { typeof(Button), "Button" },
        { typeof(Slider), "Slider" },
        { typeof(Text), "Text" },
        { typeof(Toggle), "Toggle" },
        { typeof(ToggleGroup), "ToggleGroup" },
        { typeof(InputField), "InputField" },
        { typeof(Dropdown), "Dropdown" },
        { typeof(Scrollbar), "Scrollbar" },
        { typeof(Image), "Image" },
        { typeof(RawImage), "RawImage" },
        { typeof(ScrollRectEx), "ScrollRectEx" },
        { typeof(ScrollRect), "ScrollRect" },
        { typeof(InfinityScrollGroup), "InfinityScrollGroup" },
    };

    private string GetComponentType(Transform root)
    {
        if (root.name.StartsWith("Obj"))
        {
            return "GameObject";
        }
        else if (root.name.StartsWith("Trans"))
        {
            return "Transform";
        }
        else if (root.name.StartsWith("Rt"))
        {
            return "RectTransform";
        }

        foreach (var i in types)
        {
            var t = root.GetComponent(i.Key);
            if (t == null)
                continue;
            return i.Value;
        }
        
        return string.Empty;
    }

    private void Init(Transform root)
    {
        for (int i = 0; i < root.childCount; i++)
        {
            var child = root.GetChild(i);

            if (child.gameObject.CompareTag(EXPORT_TAG))
            {
                var type = GetComponentType(child);

                var path = GetObjPath(child);
                infoList.Add(new ComponentInfo(child.name, path, type));
            }

            Init(child);
        }
    }

    private string GetObjPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null && !transform.parent.name.Equals(currentSelect.name))
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }

    private const string EXPORT_TAG = "Export";

    private List<ComponentInfo> infoList = new List<ComponentInfo>();
    public class ComponentInfo
    {
        public string Name { get; private set; }
        public string Path { get; private set; }
        public string Type { get; private set; }

        public ComponentInfo(string name, string path, string type)
        {
            this.Name = name;
            this.Path = path;
            this.Type = type;
        }
    }

}