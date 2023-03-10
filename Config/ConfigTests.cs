using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using TestLibrary.TestSupport;

namespace TestLibrary.Config {
    public class TestElement : ConfigurationElement {
        [ConfigurationProperty("ID", IsKey = true, IsRequired = true)] public String ID { get { return ((String)base["ID"]).Trim(); } }
        [ConfigurationProperty("Description", IsKey = false, IsRequired = true)] public String Description { get { return ((String)base["Description"]).Trim(); } }
        [ConfigurationProperty("Revision", IsKey = false, IsRequired = true)] public String Revision { get { return ((String)base["Revision"]).Trim(); } }
        [ConfigurationProperty("ClassName", IsKey = false, IsRequired = true)] public String ClassName { get { return ((String)base["ClassName"]).Trim(); } }
        [ConfigurationProperty("Arguments", IsKey = false, IsRequired = true)] public String Arguments { get { return ((String)base["Arguments"]).Trim(); } }
    }

    [ConfigurationCollection(typeof(TestElement))]
    public class TestElements : ConfigurationElementCollection {
        public const String PropertyName = "TestElement";
        public TestElement this[Int32 idx] { get { return (TestElement)BaseGet(idx); } }
        public override ConfigurationElementCollectionType CollectionType { get { return ConfigurationElementCollectionType.BasicMapAlternate; } }
        protected override String ElementName { get { return PropertyName; } }
        protected override bool IsElementName(String elementName) { return elementName.Equals(PropertyName, StringComparison.InvariantCultureIgnoreCase); }
        public override bool IsReadOnly() { return false; }
        protected override ConfigurationElement CreateNewElement() { return new TestElement(); }
        protected override object GetElementKey(ConfigurationElement element) { return ((TestElement)(element)).ID; }
    }

    public class TestElementsSection : ConfigurationSection {
        [ConfigurationProperty("TestElements")] public TestElements TestElements { get { return ((TestElements)(base["TestElements"])); } }
    }

    public class ConfigTests {
        public TestElementsSection TestElementsSection { get { return (TestElementsSection)ConfigurationManager.GetSection("TestElementsSection"); } }
        public TestElements TestElements { get { return this.TestElementsSection.TestElements; } }
        public IEnumerable<TestElement> TestElement { get { foreach (TestElement te in this.TestElements) if (te != null) yield return te; } }
    }

    public abstract class TestAbstract {
        public const String ClassName = nameof(TestAbstract);
        private protected TestAbstract() { }

        public static Dictionary<String, String> SplitArguments(String arguments) {
            String[] args = arguments.Split(Test.SPLIT_ARGUMENTS_CHAR);
            String[] kvp;
            Dictionary<String, String> argDictionary = new Dictionary<String, String>();
            for (int i = 0; i < args.Length; i++) {
                kvp = args[i].Split('=');
                argDictionary.Add(kvp[0].Trim(), kvp[1].Trim());
            }
            return argDictionary;
        }
    }

    public class TestCustomizable : TestAbstract {
        public new const String ClassName = nameof(TestCustomizable);
        public Dictionary<String, String> Arguments;

        public TestCustomizable(String id, String arguments) {
            this.Arguments = TestAbstract.SplitArguments(arguments);
            if (this.Arguments.Count == 0) throw new ArgumentException($"TestElement ID '{id}' with ClassName '{ClassName}' requires 1 or more key=value arguments:{Environment.NewLine}" +
                    $"   Example: 'NameFirst=Harry|" +
                    $"             NameLast=Potter|" +
                    $"             Occupation=Auror'{Environment.NewLine}" +
                    $"   Actual : '{Arguments}'");
        }
    }

    public class TestISP : TestAbstract {
        public new const String ClassName = nameof(TestISP);
        public String ISPExecutableFolder;
        public String ISPExecutable;
        public String ISPExecutableArguments;
        public String ISPResult;

        public TestISP(String id, String arguments) {
            Dictionary<String, String> argsDict = TestAbstract.SplitArguments(arguments);
            if (argsDict.Count != 4) throw new ArgumentException($"TestElement ID '{id}' with ClassName '{ClassName}' requires 4 case-sensitive arguments:{Environment.NewLine}" +
                $@"   Example: 'ISPExecutable=ipecmd.exe|
                                ISPExecutableFolder=C:\Program Files\Microchip\MPLABX\v6.05\mplab_platform\mplab_ipe|
                                ISPExecutableArguments=C:\TBD\U1_Firmware.hex|
                                ISPResult=0xAC0E'{Environment.NewLine}" +
                $"   Actual : '{arguments}'");
            if (!argsDict.ContainsKey("ISPExecutableFolder")) throw new ArgumentException($"TestElement ID '{id}' does not contain 'ISPExecutableFolder' key-value pair.");
            if (!argsDict.ContainsKey("ISPExecutable")) throw new ArgumentException($"TestElement ID '{id}' does not contain 'ISPExecutable' key-value pair.");
            if (!argsDict.ContainsKey("ISPExecutableArguments")) throw new ArgumentException($"TestElement ID '{id}' does not contain 'ISPExecutableArguments' key-value pair.");
            if (!argsDict.ContainsKey("ISPResult")) throw new ArgumentException($"TestElement ID '{id}' does not contain 'ISPResult' key-value pair.");
            if (!argsDict["ISPExecutableFolder"].EndsWith(@"\")) argsDict["ISPExecutableFolder"] += @"\";
            if (!Directory.Exists(argsDict["ISPExecutableFolder"])) throw new ArgumentException($"TestElement ID '{id}' ISPExecutableFolder '{argsDict["ISPExecutableFolder"]}' does not exist.");
            if (!File.Exists(argsDict["ISPExecutableFolder"] + argsDict["ISPExecutable"])) throw new ArgumentException($"TestElement ID '{id}' ISPExecutable '{argsDict["ISPExecutableFolder"] + argsDict["ISPExecutable"]}' does not exist.");

            this.ISPExecutableFolder = argsDict["ISPExecutableFolder"];
            this.ISPExecutable = argsDict["ISPExecutable"];
            this.ISPExecutableArguments = argsDict["ISPExecutableArguments"];
            this.ISPResult = argsDict["ISPResult"];
        }
    }

    public class TestNumerical : TestAbstract {
        public new const String ClassName = nameof(TestNumerical);
        public Double High { get; private set; }
        public Double Low { get; private set; }
        public String Unit { get; private set; }
        public String UnitType { get; private set; }

        public TestNumerical(String id, String arguments) {
            Dictionary<String, String> argsDict = TestAbstract.SplitArguments(arguments);
            if (argsDict.Count != 4) throw new ArgumentException($"TestElement ID '{id}' with ClassName '{ClassName}' requires 4 case-sensitive arguments:{Environment.NewLine}" +
                $"   Example: 'High=0.004|" +
                $"             Low=0.002|" +
                $"             Unit=A|" +
                $"             UnitType=DC'{Environment.NewLine}" +
                $"   Actual : '{arguments}'");
            if (!argsDict.ContainsKey("High")) throw new ArgumentException($"TestElement ID '{id}' does not contain 'High' key-value pair.");
            if (!argsDict.ContainsKey("Low")) throw new ArgumentException($"TestElement ID '{id}' does not contain 'Low' key-value pair.");
            if (!argsDict.ContainsKey("Unit")) throw new ArgumentException($"TestElement ID '{id}' does not contain 'Unit' key-value pair.");
            if (!argsDict.ContainsKey("UnitType")) throw new ArgumentException($"TestElement ID '{id}' does not contain 'UnitType' key-value pair.");

            if (Double.TryParse(argsDict["High"], NumberStyles.Float, CultureInfo.CurrentCulture, out Double high)) this.High = high;
            else throw new ArgumentException($"TestElement ID '{id}' High '{argsDict["High"]}' ≠ System.Double.");

            if (Double.TryParse(argsDict["Low"], NumberStyles.Float, CultureInfo.CurrentCulture, out Double low)) this.Low = low;
            else throw new ArgumentException($"TestElement ID '{id}' Low '{argsDict["Low"]}' ≠ System.Double.");

            if (low > high) throw new ArgumentException($"TestElement ID '{id}' Low '{low}' > High '{high}'.");
            this.Unit = argsDict["Unit"];
            this.UnitType = argsDict["UnitType"];
        }
    }

    public class TestTextual : TestAbstract {
        public new const String ClassName = nameof(TestTextual);
        public String Text { get; private set; }
        public TestTextual(String id, String arguments) {
            Dictionary<String, String> argsDict = TestAbstract.SplitArguments(arguments);
            if (argsDict.Count != 1) throw new ArgumentException($"TestElement ID '{id}' with ClassName '{ClassName}' requires 1 case-sensitive argument:{Environment.NewLine}" +
                    $"   Example: 'Text=The quick brown fox jumps over the lazy dog.'{Environment.NewLine}" +
                    $"   Actual : '{arguments}'");
            if (!argsDict.ContainsKey("Text")) throw new ArgumentException($"TestElement ID '{id}' does not contain 'Text' key-value pair.");
            this.Text = argsDict["Text"];
        }
    }

    public class Test {
        internal const Char SPLIT_ARGUMENTS_CHAR = '|';
        public String ID { get; private set; }
        public String Description { get; private set; }
        public String Revision { get; private set; }
        public String ClassName { get; private set; }
        public object ClassObject { get; private set; }
        public String Measurement { get; set; }
        public String Result { get; set; }

        private Test(String id, String description, String revision, String className, String arguments) {
            this.ID = id;
            this.Description = description;
            this.Revision = revision;
            this.ClassName = className;
            if (String.Equals(this.ClassName, TestNumerical.ClassName)) this.Measurement = Double.NaN.ToString();
            else this.Measurement = String.Empty;
            this.Result = EventCodes.UNSET;
            this.ClassObject = Activator.CreateInstance(Type.GetType(GetType().Namespace + "." + this.ClassName), new Object[] { this.ID, arguments });
        }

        public static Dictionary<String, Test> Get() {
            TestElementsSection testElementsSection = (TestElementsSection)ConfigurationManager.GetSection("TestElementsSection");
            TestElements testElements = testElementsSection.TestElements;
            Dictionary<String, Test> dictionary = new Dictionary<String, Test>();
            foreach (TestElement testElement in testElements) dictionary.Add(testElement.ID, new Test(testElement.ID, testElement.Description, testElement.Revision, testElement.ClassName, testElement.Arguments));
            return dictionary;
        }
    }

    public class ConfigTest {
        public Group Group { get; private set; }
        public Dictionary<String, Test> Tests { get; private set; }
        public Int32 LogFormattingLength { get; private set; }

        private ConfigTest() {
            Dictionary<String, Group> Groups = Group.Get();
            String GroupSelected = GroupSelect.Get(Groups);
            this.Group = Groups[GroupSelected];
            // Operator selects the Group they want to test, from the Dictionary of all Groups.
            // GroupSelected is Dictionary Groups' Key.

            Dictionary<String, Test> tests = Test.Get();
            this.Tests = new Dictionary<String, Test>();
            String[] g = this.Group.TestIDs.Split(Test.SPLIT_ARGUMENTS_CHAR);
            String sTrim;
            this.LogFormattingLength = 0;
            foreach (String s in g) {
                sTrim = s.Trim();
                if (sTrim.Length > this.LogFormattingLength) this.LogFormattingLength = sTrim.Length;
                if (!tests.ContainsKey(sTrim)) throw new InvalidOperationException($"Group '{Group.ID}' includes IDTest '{sTrim}', which isn't present in TestElements in App.config.");
                this.Tests.Add(sTrim, tests[sTrim]);
                // Add only Tests correlated to the Group previously selected by operator.
            }
            this.LogFormattingLength += 1; // Leave room for a space.
        }

        public static ConfigTest Get() { return new ConfigTest(); }
    }
}
