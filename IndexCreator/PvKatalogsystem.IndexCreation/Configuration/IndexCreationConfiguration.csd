<?xml version="1.0" encoding="utf-8"?>
<configurationSectionModel xmlns:dm0="http://schemas.microsoft.com/VisualStudio/2008/DslTools/Core" dslVersion="1.0.0.0" Id="3fbd8b8c-9110-407b-9c75-aef7495bb743" namespace="PvKatalogsystem.IndexCreation.Configuration" xmlSchemaNamespace="urn:PvKatalogsystem.IndexCreation.Configuration" assemblyName="PvKatalogsystem.IndexCreation" xmlns="http://schemas.microsoft.com/dsltools/ConfigurationSectionDesigner">
  <typeDefinitions>
    <externalType name="String" namespace="System" />
    <externalType name="Boolean" namespace="System" />
    <externalType name="Int32" namespace="System" />
    <externalType name="Int64" namespace="System" />
    <externalType name="Single" namespace="System" />
    <externalType name="Double" namespace="System" />
    <externalType name="DateTime" namespace="System" />
    <externalType name="TimeSpan" namespace="System" />
  </typeDefinitions>
  <configurationElements>
    <configurationSection name="IndexCreationConfig" codeGenOptions="Singleton, XmlnsProperty" xmlSectionName="indexCreationConfig">
      <elementProperties>
        <elementProperty name="MasterServer" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="masterServer" isReadOnly="false">
          <type>
            <configurationElementMoniker name="/3fbd8b8c-9110-407b-9c75-aef7495bb743/MasterServerConfig" />
          </type>
        </elementProperty>
        <elementProperty name="ReplicationServer" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="replicationServer" isReadOnly="false">
          <type>
            <configurationElementCollectionMoniker name="/3fbd8b8c-9110-407b-9c75-aef7495bb743/ReplicationServerCollection" />
          </type>
        </elementProperty>
        <elementProperty name="Indices" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="indices" isReadOnly="false">
          <type>
            <configurationElementCollectionMoniker name="/3fbd8b8c-9110-407b-9c75-aef7495bb743/IndexCollection" />
          </type>
        </elementProperty>
        <elementProperty name="Database" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="database" isReadOnly="false">
          <type>
            <configurationElementMoniker name="/3fbd8b8c-9110-407b-9c75-aef7495bb743/DatabaseConfig" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationSection>
    <configurationElement name="MasterServerConfig">
      <attributeProperties>
        <attributeProperty name="Uri" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="uri" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/3fbd8b8c-9110-407b-9c75-aef7495bb743/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElementCollection name="ReplicationServerCollection" xmlItemName="server" codeGenOptions="GetItemMethods">
      <itemType>
        <configurationElementMoniker name="/3fbd8b8c-9110-407b-9c75-aef7495bb743/ReplicationServerConfig" />
      </itemType>
    </configurationElementCollection>
    <configurationElement name="ReplicationServerConfig">
      <attributeProperties>
        <attributeProperty name="Name" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="name" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/3fbd8b8c-9110-407b-9c75-aef7495bb743/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Uri" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="uri" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/3fbd8b8c-9110-407b-9c75-aef7495bb743/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElement name="IndexConfig">
      <attributeProperties>
        <attributeProperty name="RelativeUri" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="relativeUri" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/3fbd8b8c-9110-407b-9c75-aef7495bb743/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="ManagementTable" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="managementTable" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/3fbd8b8c-9110-407b-9c75-aef7495bb743/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="Name" isRequired="true" isKey="true" isDefaultCollection="false" xmlName="name" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/3fbd8b8c-9110-407b-9c75-aef7495bb743/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="ChunkSize" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="chunkSize" isReadOnly="false" defaultValue="10000">
          <type>
            <externalTypeMoniker name="/3fbd8b8c-9110-407b-9c75-aef7495bb743/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="Replication" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="replication" isReadOnly="false" defaultValue="false">
          <type>
            <externalTypeMoniker name="/3fbd8b8c-9110-407b-9c75-aef7495bb743/Boolean" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElementCollection name="IndexCollection" xmlItemName="index" codeGenOptions="GetItemMethods">
      <attributeProperties>
        <attributeProperty name="Threads" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="threads" isReadOnly="false" defaultValue="2">
          <type>
            <externalTypeMoniker name="/3fbd8b8c-9110-407b-9c75-aef7495bb743/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="Namespace" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="namespace" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/3fbd8b8c-9110-407b-9c75-aef7495bb743/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
      <itemType>
        <configurationElementMoniker name="/3fbd8b8c-9110-407b-9c75-aef7495bb743/IndexConfig" />
      </itemType>
    </configurationElementCollection>
    <configurationElement name="DatabaseConfig">
      <attributeProperties>
        <attributeProperty name="SourceConnectionString" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="sourceConnectionString" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/3fbd8b8c-9110-407b-9c75-aef7495bb743/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="ManagementConnectionString" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="managementConnectionString" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/3fbd8b8c-9110-407b-9c75-aef7495bb743/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
  </configurationElements>
  <propertyValidators>
    <validators />
  </propertyValidators>
</configurationSectionModel>