
// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://tempuri.org/SettingsSchema.xsd")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://tempuri.org/SettingsSchema.xsd", IsNullable = false)]
public partial class Settings
{

    private SettingsCommon commonField;

    private SettingsEnviroment[] enviromentsField;

    /// <remarks/>
    public SettingsCommon Common
    {
        get
        {
            return this.commonField;
        }
        set
        {
            this.commonField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("Enviroment", IsNullable = false)]
    public SettingsEnviroment[] Enviroments
    {
        get
        {
            return this.enviromentsField;
        }
        set
        {
            this.enviromentsField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://tempuri.org/SettingsSchema.xsd")]
public partial class SettingsCommon
{

    private string scriptsFilesPathField;

    private string postDeploymentScriptsFilesPathField;

    private SettingsCommonTargetTempDataBase targetTempDataBaseField;

    /// <remarks/>
    public string ScriptsFilesPath
    {
        get
        {
            return this.scriptsFilesPathField;
        }
        set
        {
            this.scriptsFilesPathField = value;
        }
    }

    /// <remarks/>
    public string PostDeploymentScriptsFilesPath
    {
        get
        {
            return this.postDeploymentScriptsFilesPathField;
        }
        set
        {
            this.postDeploymentScriptsFilesPathField = value;
        }
    }

    /// <remarks/>
    public SettingsCommonTargetTempDataBase TargetTempDataBase
    {
        get
        {
            return this.targetTempDataBaseField;
        }
        set
        {
            this.targetTempDataBaseField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://tempuri.org/SettingsSchema.xsd")]
public partial class SettingsCommonTargetTempDataBase
{

    private string userField;

    private string passwordField;

    private ushort portField;

    private string hostField;

    private string databaseNameField;

    /// <remarks/>
    public string User
    {
        get
        {
            return this.userField;
        }
        set
        {
            this.userField = value;
        }
    }

    /// <remarks/>
    public string Password
    {
        get
        {
            return this.passwordField;
        }
        set
        {
            this.passwordField = value;
        }
    }

    /// <remarks/>
    public ushort Port
    {
        get
        {
            return this.portField;
        }
        set
        {
            this.portField = value;
        }
    }

    /// <remarks/>
    public string Host
    {
        get
        {
            return this.hostField;
        }
        set
        {
            this.hostField = value;
        }
    }

    /// <remarks/>
    public string DatabaseName
    {
        get
        {
            return this.databaseNameField;
        }
        set
        {
            this.databaseNameField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://tempuri.org/SettingsSchema.xsd")]
public partial class SettingsEnviroment
{

    private SettingsEnviromentTargetDataBase targetDataBaseField;

    private string nameField;

    private bool executeOnTargetField;

    /// <remarks/>
    public SettingsEnviromentTargetDataBase TargetDataBase
    {
        get
        {
            return this.targetDataBaseField;
        }
        set
        {
            this.targetDataBaseField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool ExecuteOnTarget
    {
        get
        {
            return this.executeOnTargetField;
        }
        set
        {
            this.executeOnTargetField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://tempuri.org/SettingsSchema.xsd")]
public partial class SettingsEnviromentTargetDataBase
{

    private string userField;

    private string passwordField;

    private ushort portField;

    private string hostField;

    private string databaseNameField;

    /// <remarks/>
    public string User
    {
        get
        {
            return this.userField;
        }
        set
        {
            this.userField = value;
        }
    }

    /// <remarks/>
    public string Password
    {
        get
        {
            return this.passwordField;
        }
        set
        {
            this.passwordField = value;
        }
    }

    /// <remarks/>
    public ushort Port
    {
        get
        {
            return this.portField;
        }
        set
        {
            this.portField = value;
        }
    }

    /// <remarks/>
    public string Host
    {
        get
        {
            return this.hostField;
        }
        set
        {
            this.hostField = value;
        }
    }

    /// <remarks/>
    public string DatabaseName
    {
        get
        {
            return this.databaseNameField;
        }
        set
        {
            this.databaseNameField = value;
        }
    }
}

