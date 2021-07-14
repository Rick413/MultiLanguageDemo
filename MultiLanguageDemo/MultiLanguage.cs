using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace MultiLanguageDemo
{

    /// <summary>
    /// 窗体属性 以及窗体控件属性集合
    /// </summary>
    public class FormGroup
    {
        //public string FormName { get; set; }
        /// <summary>
        /// 属性：（属性名称-对应的中英文)
        /// </summary>
        public Dictionary<string, DynamicModel> DicFormTranslate = new Dictionary<string, DynamicModel>();
        //public Dictionary<string, TextTranslate> DicFormAttribute { get; set; }
        /// <summary>
        /// 窗体里的控件属性集合
        /// </summary>
        public Dictionary<string, ControlGroup> DicControlAttribute = new Dictionary<string, ControlGroup>();
        //public Dictionary<string, ControlGroup> DicControlAttribute { get; set; }
    }

    /// <summary>
    /// 窗体控件属性Group
    /// </summary>
    public class ControlGroup
    {
        /// <summary>
        /// 属性：（属性名称-对应的中英文)
        /// </summary>
        public Dictionary<string, DynamicModel> DicControlTranslate = new Dictionary<string, DynamicModel>();
    }
    /// <summary>
    /// 翻译文本
    /// </summary>
    //public class TextTranslate 
    //{
    //    public Dictionary<string ,string> Translate { get; set; }
    //    /// <summary>
    //    /// 中文
    //    /// </summary>
    //    public string CN { get; set; }
    //    public string EN { get; set; }
    //}

    #region 动态Model类

    public class DynamicModel : DynamicObject
    {
        private string propertyName;
        public string PropertyName
        {
            get { return propertyName; }
            set { propertyName = value; }
        }

        // The inner dictionary.
        Dictionary<string, object> dicProperty
            = new Dictionary<string, object>();
        public Dictionary<string, object> DicProperty
        {
            get
            {
                return dicProperty;
            }
        }


        // This property returns the number of elements
        // in the inner dictionary.
        public int Count
        {
            get
            {
                return dicProperty.Count;
            }
        }

        // If you try to get a value of a property 
        // not defined in the class, this method is called.
        public override bool TryGetMember(
            GetMemberBinder binder, out object result)
        {
            // Converting the property name to lowercase
            // so that property names become case-insensitive.
            string name = binder.Name;

            // If the property name is found in a dictionary,
            // set the result parameter to the property value and return true.
            // Otherwise, return false.
            return dicProperty.TryGetValue(name, out result);
        }

        // If you try to set a value of a property that is
        // not defined in the class, this method is called.
        public override bool TrySetMember(
            SetMemberBinder binder, object value)
        {
            // Converting the property name to lowercase
            // so that property names become case-insensitive.
            if (binder.Name == "Property")
            {
                dicProperty[PropertyName] = value;
            }
            else
            {
                dicProperty[binder.Name] = value;
            }


            // You can always add a value to a dictionary,
            // so this method always returns true.
            return true;
        }
    }

    #endregion



    public class MultiLanguage : SingletonTemplate<MultiLanguage>
    {
        /// <summary>
        /// 所有界面文本数据
        /// </summary>
        private Dictionary<string, FormGroup> DicForm = new Dictionary<string, FormGroup>();
        private List<string> language = new List<string>();
        /// <summary>
        /// 语言列表——combox绑定数据
        /// </summary>
        public List<string> Language
        {
            get
            {
                return language;
            }
        }
        //从xml中读取的可切换语言类型

        //public static Dictionary<string, FormGroup> GetText(XElement loadElement)
        //{
        //    return (from element in loadElement.Elements("Form")
        //            select new Dictionary<string, FormGroup>()
        //                {
        //                    {(string) element.Attribute("Name"), new FormGroup() { }}
        //                })
        //        .ToDictionary(key => key.Keys, value => value.Values);
        //}
        /// <summary>
        /// 加载翻译文本xml
        /// </summary>
        /// <param name="strFile">xml文本地址</param>
        /// <returns></returns>
        public bool LoadParamFile(string strFile)
        {
            XmlDocument document = new XmlDocument();
            XmlNodeList mainNodes;
            XmlNodeList childNodes;
            if (!File.Exists(strFile))
            {
                return false;
            }

            try
            {
                XElement loadElement = XElement.Load(strFile);
                var options = (from element in loadElement.Descendants("Options").Elements()
                               select
                                   element).ToList();
                foreach (var languageXe in options)
                {
                    language.Add(languageXe.Value);
                }
                //动态添加属性
                var result = from element in loadElement.Elements("Form").Elements() select element;

                foreach (var VARIABLE in result)
                {
                    DicForm.Add((string)VARIABLE.Attribute("Name"), new FormGroup());
                }

                foreach (var xe in result) //遍历窗体
                {
                    //DicFormTranslate
                    string FormName = (string)xe.Attribute("Name");
                    if (DicForm.ContainsKey(FormName))
                    {
                        //获取窗体属性
                        var formItme = from element in xe.Elements("Item") select element;
                        foreach (var itme in formItme)
                        {
                            dynamic dynamicModel = new DynamicModel();
                            foreach (string text in Language)
                            {
                                dynamicModel.PropertyName = text;
                                dynamicModel.Property = (string)itme.Attribute(text);
                            }

                            DicForm[FormName].DicFormTranslate.Add((string)itme.Attribute("属性"), dynamicModel);
                        }

                        //获取控件属性
                        var controlList = from element in xe.Elements("Control") select element;
                        if (controlList.Count() > 0)
                        {
                            foreach (var VARIABLE in controlList)
                            {
                                string str = (string)VARIABLE.Attribute("Name");
                                DicForm[FormName].DicControlAttribute.Add((string)VARIABLE.Attribute("Name"),
                                    new ControlGroup());
                            }

                            foreach (var control in controlList)
                            {
                                string ControlName = (string)control.Attribute("Name");
                                var ControlItmeS = from el in control.Elements("Item") select el;
                                foreach (var controlItme in ControlItmeS)
                                {
                                    dynamic dynamicModel2 = new DynamicModel();
                                    foreach (string text2 in Language)
                                    {
                                        dynamicModel2.PropertyName = text2;
                                        dynamicModel2.Property = (string)controlItme.Attribute(text2);
                                    }
                                    DicForm[FormName].DicControlAttribute[ControlName].DicControlTranslate.Add((string)controlItme.Attribute("属性"), dynamicModel2
                                    );
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return false;
            }

            //TODO：在此处查看读取数据--------------------------------------------------测试
            #region Test

            foreach (string formStr in DicForm.Keys)
            {
                //窗体名称
                Console.WriteLine($"窗体名称:{formStr}");
                //窗体属性
                Console.WriteLine("窗体属性:");
                foreach (string formConStr in DicForm[formStr].DicFormTranslate.Keys)
                {
                    dynamic dform = DicForm[formStr].DicFormTranslate[formConStr];
                    Console.WriteLine($"{formConStr}:中文：{dform.CN}|英文：{dform.DicProperty["EN"].ToString()}");
                }
                // 控件
                foreach (string conStr in DicForm[formStr].DicControlAttribute.Keys)
                {
                    //控件名称
                    Console.WriteLine($"控件名称：{conStr}");
                    // 控件属性
                    foreach (string conAttStr in DicForm[formStr].DicControlAttribute[conStr].DicControlTranslate.Keys)
                    {
                        Console.WriteLine($"控件属性：{conAttStr}");
                        dynamic dcontrol = DicForm[formStr].DicControlAttribute[conStr].DicControlTranslate[conAttStr];
                        Console.WriteLine($"{Language[0]}：{dcontrol.DicProperty[Language[0]].ToString()}|{Language[1]}：{dcontrol.DicProperty[Language[1]].ToString()}");
                    }
                }
            }

            #endregion

            return true;
        }

        public string CurrLanguage { get; set; }
        public bool SelectLanguage(Form form, string selectLanguage = "")
        {
            if (selectLanguage == "")
            {
                selectLanguage = string.IsNullOrEmpty(CurrLanguage) ? "" : CurrLanguage;
                if (selectLanguage == "")
                {
                    return false;
                }
            }
            string language = Language.Find(i => i == selectLanguage);
            string formStr = form?.Name;
            //【1】窗体存在
            if (DicForm.ContainsKey(form?.Name) && !string.IsNullOrEmpty(language))
            {
                try
                {
                    CurrLanguage = language;
                    FormGroup fromGroup = DicForm[formStr];
                    PropertyInfo pi;
                    //【2】修改窗体_属性
                    foreach (string formAttStr in fromGroup.DicFormTranslate.Keys)
                    {
                        dynamic d = fromGroup.DicFormTranslate[formAttStr];//提取动态类
                        pi = form.GetType().GetProperty(formAttStr);//获取窗体属性信息
                        pi?.SetValue(form, d.DicProperty[language]);//窗体属性不为空则赋值
                    }
                    //【3】修改控件_属性
                    UpdateControls(form.Controls, fromGroup.DicControlAttribute, fromGroup.DicControlAttribute.Keys.ToList());

                }
                catch (Exception e)
                {
                    return false;
                }
                return true;
            }

            return false;
        }

        public void UpdateControls(Control.ControlCollection controls, Dictionary<string, ControlGroup> DicControlsAtt, List<string> conList)
        {
            if (conList.Count <= 0)//【1】需要处理的控件列表
            {
                return;
            }
            try
            {
                foreach (Control control in controls)//【2】遍历控件
                {
                    if (conList.Count <= 0)//退出递归
                    {
                        return;
                    }
                    ////todo:ForTest查看窗体控件集合
                    //if (control.Name == "lb_DeviceModel")
                    //{
                    //    Console.WriteLine();
                    //}
                    ////
                    if (conList.Contains(control.Name))//【3】判断当前控件是否在[待处理列表]
                    {
                        PropertyInfo pitemp;
                        foreach (string conAtt in DicControlsAtt[control.Name].DicControlTranslate.Keys)//遍历控件属性
                        {
                            pitemp = control.GetType().GetProperty(conAtt);
                            dynamic d = DicControlsAtt[control.Name].DicControlTranslate[conAtt];
                            if (conAtt == "Font")
                            {
                                pitemp?.SetValue(control, new System.Drawing.Font("微软雅黑", Convert.ToSingle(d.DicProperty[CurrLanguage])));//窗体属性不为空则赋值
                            }
                            else
                            {
                                pitemp?.SetValue(control, d.DicProperty[CurrLanguage]);//窗体属性不为空则赋值
                            }
                        }

                        conList.Remove(control.Name);
                    }
                    else //【4】当前控件不在待处理列表，就需要判断控件类型
                    {
                        ////判断是什么控件
                        //if (control.GetType() == typeof(MenuStrip))
                        //{
                        //}else if (control.GetType() == typeof(MenuStrip))
                        //{
                        //}

                        //todo:此处添加不同控件修改文本方式
                        //不同控件类型修改文本方式不同
                        switch (control.GetType().Name)
                        {
                            case "MenuStrip":
                                foreach (ToolStripItem tsi in ((MenuStrip)control).Items)
                                {

                                    PropertyInfo tsitemp;
                                    tsitemp = tsi.GetType().GetProperty("Name");
                                    //获取控件名称，
                                    string tsiName = tsitemp.GetValue(tsi).ToString();

                                    if (conList.Contains(tsiName))//【3】判断当前控件是否在[待处理列表]
                                    {
                                        foreach (string conAtt in DicControlsAtt[tsiName].DicControlTranslate.Keys)//遍历控件属性
                                        {
                                            tsitemp = tsi.GetType().GetProperty(conAtt);
                                            dynamic d = DicControlsAtt[tsiName].DicControlTranslate[conAtt];
                                            tsitemp?.SetValue(tsi, d.DicProperty[CurrLanguage]);//窗体属性不为空则赋值
                                        }
                                        conList.Remove(tsiName);
                                    }
                                    //判断是否需要递归 ToolStripItem
                                    if (tsi is ToolStripMenuItem)
                                    {
                                        if (((ToolStripMenuItem)tsi).DropDownItems.Count > 0)//非Controls的递归
                                        {
                                            Recursion(tsi, DicControlsAtt, conList);
                                        }
                                    }

                                }
                                break;
                            case "ListView":
                                foreach (ColumnHeader ch in ((ListView)control).Columns)
                                {
                                    //先处理，在判断是否需要递归
                                    //ToolStripMenuItem
                                    PropertyInfo chtemp;
                                    //chtemp = ch.GetType().GetProperty("Name");
                                    //获取控件名称，
                                    string tsiName = ch.Name;
                                    if (conList.Contains(tsiName))//【3】判断当前控件是否在[待处理列表]
                                    {
                                        foreach (string conAtt in DicControlsAtt[tsiName].DicControlTranslate.Keys)//遍历控件属性
                                        {
                                            chtemp = ch.GetType().GetProperty(conAtt);
                                            dynamic d = DicControlsAtt[tsiName].DicControlTranslate[conAtt];
                                            chtemp?.SetValue(ch, d.DicProperty[CurrLanguage]);//窗体属性不为空则赋值
                                        }
                                        conList.Remove(tsiName);
                                    }
                                }
                                break;
                            case "Panel":

                            default:

                                break;
                        }
                    }

                    PropertyInfo pi = control.GetType().GetProperty("Controls");//存在Controls
                    if (pi != null && ((Control.ControlCollection)pi.GetValue(control)).Count > 0)
                    {
                        UpdateControls((Control.ControlCollection)pi.GetValue(control), DicControlsAtt, conList);
                    }
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine(e);
                throw e;
            }
        }



        public void Recursion(object con, Dictionary<string, ControlGroup> DicControlsAtt, List<string> conList)
        {
            if (conList.Count <= 0)//需要处理的控件列表
            {
                return;
            }
            //todo:此处添加不同控件递归方式
            switch (con.GetType().Name)
            {
                case "ToolStripMenuItem":
                    ToolStripMenuItem control = (ToolStripMenuItem)con;
                    foreach (ToolStripItem tsi in control.DropDownItems)
                    {
                        PropertyInfo tsitemp;
                        tsitemp = tsi.GetType().GetProperty("Name");
                        //获取控件名称，
                        string tsiName = tsitemp.GetValue(tsi).ToString();
                        if (conList.Contains(tsiName))//【3】判断当前控件是否在[待处理列表]
                        {
                            foreach (string conAtt in DicControlsAtt[tsiName].DicControlTranslate.Keys)//遍历控件属性
                            {
                                tsitemp = tsi.GetType().GetProperty(conAtt);
                                dynamic d = DicControlsAtt[tsiName].DicControlTranslate[conAtt];
                                tsitemp?.SetValue(tsi, d.DicProperty[CurrLanguage]);//窗体属性不为空则赋值
                            }
                            conList.Remove(tsiName);
                        }

                        if (tsi is ToolStripMenuItem)
                        {
                            if (((ToolStripMenuItem)tsi).DropDownItems.Count > 0)//非Controls的递归
                            {
                                Recursion(tsi, DicControlsAtt, conList);
                            }
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        public bool UpdateText(Form form, string language)
        {
            Type type = form.GetType();
            PropertyInfo per = type.GetProperty("Text");
            per?.SetValue(form, "界面");
            return true;
        }

    }
}

#region 功能说明

/// <summary>
/////实现自动遍历控件Controls，查找需要修改的控件Name属性去做匹配
///  没有Name或者其他特殊情况，特殊控件等需要自己实现
/// 当前已实现
/// 1、panel
/// 2、MenuStrip
/// 3、ListView 的 Columns集合的ColumnHeader 需要自己添加Name属性（重要）
/// 
/// </summary>

#endregion







public class SingletonTemplate<T> where T : class, new()
{
    private static T instance;
    private static readonly object objlock = new object();

    public static T GetInstance()
    {
        if (SingletonTemplate<T>.instance == null)
        {
            lock (objlock)
            {
                if (SingletonTemplate<T>.instance == null)
                {
                    SingletonTemplate<T>.instance = Activator.CreateInstance<T>();
                }
            }
        }
        return SingletonTemplate<T>.instance;
    }

}
