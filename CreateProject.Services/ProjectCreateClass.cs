using CreateProject.Data;
using CreateProject.Models;
using CreateProject.Tools.String;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CreateProject.Services
{
    public class ProjectCreateClass
    {
        private readonly IRepositoryProject _repository;
        private List<TablesView> tables = new List<TablesView>();

        public ProjectCreateClass()
        {
            _repository = new RepositoryProject();
            tables = ConstructTables();
        }

        private List<TablesView> GetTables()
        {
            try
            {
                var tables = _repository.GetTables();
                return tables;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private List<PropertiesView> GetProperties(string tableName)
        {
            try
            {
                var properties = _repository.GetProperties(tableName);
                return properties;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private List<TablesView> ConstructTables()
        {
            try
            {
                var tablesTemp = GetTables();
                var schema = "SchemaDb".ReadAppConfig("dbo");
                var nameSapaceProject = "NameSpaceProject".ReadAppConfig();

                foreach (var table in tablesTemp)
                {
                    table.NameSpace = nameSapaceProject;
                    var properties = GetProperties(table.TableName);
                    table.PrimaryKey = _repository.GetPrimaryKey(table.TableName, schema);
                    table.Properties = properties;
                }
                return tablesTemp;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void CreateClassModels()
        {
            try
            {
                var directoryProjectModelBase = Path.Combine("BaseDirectory".ReadAppConfig(), "Models");
                var extensionFile = ".cs";
                string nameSpace = "Models";

                if (Directory.Exists(directoryProjectModelBase))
                    Directory.Delete(directoryProjectModelBase, true);

                Directory.CreateDirectory(directoryProjectModelBase);

                foreach (var table in tables)
                {
                    string file = Path.Combine(directoryProjectModelBase, $"{table.TableName}{extensionFile}");

                    var pk = table.Properties.Where(x => x.ColumnName == table.PrimaryKey).FirstOrDefault();

                    var line = new StringBuilder();
                    line.AppendLine(@"using System;");
                    line.AppendLine(@"using System.ComponentModel.DataAnnotations;");
                    line.AppendLine(@"using System.ComponentModel.DataAnnotations.Schema;");
                    line.AppendLine(string.Empty);
                    line.AppendLine(string.Empty);
                    line.AppendLine($"namespace {table.NameSpace}.{nameSpace}");
                    line.AppendLine(@"{");
                    var nameClass = @"""" + table.TableName + "\"";
                    line.AppendLine($"\t[Table({nameClass})]");
                    line.AppendLine($"\tpublic class {table.TableName}");
                    line.AppendLine("\t{");
                    line.AppendLine($"\t\t[Key]");
                    var primaryKey = @"""" + table.PrimaryKey + "\"";
                    line.AppendLine($"\t\t[Column({primaryKey})]");
                    line.AppendLine($"\t\t {pk.Column}");
                    line.AppendLine(string.Empty);

                    foreach (var property in table.Properties.Where(x => x.ColumnName != pk.ColumnName).OrderBy(y => y.OrderColumn))
                    {
                        var propertyName = @"""" + property.ColumnName + "\"";
                        line.AppendLine($"\t\t[Column({propertyName})]");
                        line.AppendLine($"\t\t {property.Column}");
                        line.AppendLine(string.Empty);
                    }

                    line.AppendLine("\t}");
                    line.AppendLine("}");

                    File.WriteAllText(file, line.ToString());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void CreateClassRepository()
        {
            try
            {
                var directoryProjectModelBase = Path.Combine("BaseDirectory".ReadAppConfig(), "Data");
                var extensionFile = ".cs";
                string nameSpace = "Data";

                if (Directory.Exists(directoryProjectModelBase))
                    Directory.Delete(directoryProjectModelBase, true);

                Directory.CreateDirectory(directoryProjectModelBase);

                foreach (var table in tables)
                {
                    string file = Path.Combine(directoryProjectModelBase, $"{table.ClassNameRepository}{extensionFile}");

                    var pk = table.Properties.Where(x => x.ColumnName == table.PrimaryKey).FirstOrDefault();

                    var line = new StringBuilder();
                    line.AppendLine(@"using Dapper;");
                    line.AppendLine($"using {table.NameSpace}.Models;");
                    line.AppendLine(@"using System;");
                    line.AppendLine(string.Empty);
                    line.AppendLine(string.Empty);
                    line.AppendLine($"namespace {table.NameSpace}.{nameSpace}");
                    line.AppendLine(@"{");
                    line.AppendLine($"\tpublic class {table.ClassNameRepository} : RepositoryGeneric<{table.TableName}> , {table.ClassNameIRepository} , IDisposable");
                    line.AppendLine("\t{");

                    line.AppendLine($"\t\tpublic {table.ClassNameRepository}()");
                    line.AppendLine("\t\t{}");
                    line.AppendLine("\t}");
                    line.AppendLine(string.Empty);

                    line.AppendLine($"\tpublic interface {table.ClassNameIRepository} : IRepositoryGeneric<{table.TableName}> ,IDisposable");
                    line.AppendLine("\t{}");

                    line.AppendLine("}");

                    File.WriteAllText(file, line.ToString());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void CreateClassServicesModels()
        {
            try
            {
                var directoryProjectModelBase = Path.Combine("BaseDirectory".ReadAppConfig(), "Services/ModelView");
                var extensionFile = ".cs";
                string nameSpace = "Services.ModelView";
                bool isModelViewIsWCF = bool.Parse("ModelViewIsWCF".ReadAppConfig());

                if (Directory.Exists(directoryProjectModelBase))
                    Directory.Delete(directoryProjectModelBase, true);

                Directory.CreateDirectory(directoryProjectModelBase);

                foreach (var table in tables)
                {
                    string file = Path.Combine(directoryProjectModelBase, $"{table.ClassNameModelView}{extensionFile}");

                    var line = new StringBuilder();
                    line.AppendLine(@"using System;");
                    line.AppendLine(@"using System.ComponentModel.DataAnnotations;");

                    if (isModelViewIsWCF)
                        line.AppendLine("using System.Runtime.Serialization;");

                    line.AppendLine(string.Empty);
                    line.AppendLine($"namespace {table.NameSpace}.{nameSpace}");
                    line.AppendLine(@"{");
                    var className = @"""" + table.ClassNameModelView + "\"";

                    if (isModelViewIsWCF)
                        line.AppendLine($"\t[Serializable]");
                    if (isModelViewIsWCF)
                        line.AppendLine($"\t[DataContract]");

                    line.AppendLine($"\tpublic class {table.ClassNameModelView}");
                    line.AppendLine("\t{");

                    foreach (var property in table.Properties.OrderBy(y => y.OrderColumn))
                    {
                        var propertyName = @"""" + property.ColumnName + "\"";

                        if (isModelViewIsWCF)
                            line.AppendLine($"\t\t[DataMember]");

                        if (!property.IsNull)
                            line.AppendLine($"\t\t[Required]");

                        if (property.IsMaxLength)
                            line.AppendLine($"\t\t[MaxLength({property.MaximumLength})]");
                        line.AppendLine($"\t\t{property.Column}");

                        line.AppendLine(string.Empty);
                    }

                    line.AppendLine("\t}");
                    line.AppendLine("}");

                    File.WriteAllText(file, line.ToString());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private StringBuilder CreateClassResponseBase()
        {
            try
            {
                var nameSpace = "NameSpaceProject".ReadAppConfig();
                bool isModelViewIsWCF = bool.Parse("ModelViewIsWCF".ReadAppConfig());
                var lineFile = new StringBuilder();
                lineFile.AppendLine(@"using System;");
                if (isModelViewIsWCF)
                    lineFile.AppendLine(@"using System.Runtime.Serialization;");
                lineFile.AppendLine(@"");
                lineFile.AppendLine($"namespace {nameSpace}.Services.Responses");
                lineFile.AppendLine(@"{");
                if (isModelViewIsWCF)
                    lineFile.AppendLine("\t[DataContract(Name=\"ResponseData{0}\" )]");

                lineFile.AppendLine(@" public abstract class ResponseData<T>");
                lineFile.AppendLine(@"    {");
                lineFile.AppendLine(@"        public ResponseData()");
                lineFile.AppendLine(@"        {");
                lineFile.AppendLine(@"            Message = string.Empty;");
                lineFile.AppendLine(@"            Success = false;");
                lineFile.AppendLine(@"        }");
                lineFile.AppendLine(@"");
                if (isModelViewIsWCF)
                    lineFile.AppendLine(@"        [DataMember]");
                lineFile.AppendLine(@"        public string Message { get; set; }");
                if (isModelViewIsWCF)
                    lineFile.AppendLine(@"        [DataMember]");
                lineFile.AppendLine(@"        public bool Success { get; set; }");
                lineFile.AppendLine(@"");
                if (isModelViewIsWCF)
                    lineFile.AppendLine(@"        [DataMember(Name = ""Data"")]");
                lineFile.AppendLine(@"        public T Data { get; set; }");
                lineFile.AppendLine(@"");
                if (isModelViewIsWCF)
                    lineFile.AppendLine(@"        [DataMember(Name = ""TypeResponse"")]");
                lineFile.AppendLine(@"        public TypeResponse TypeResponse { get; set; } = TypeResponse.Na;");
                lineFile.AppendLine(@"");
                lineFile.AppendLine(@"        public void Warning(string errorMessage)");
                lineFile.AppendLine(@"        {");
                lineFile.AppendLine(@"            Message = errorMessage;");
                lineFile.AppendLine(@"            Success = false;");
                lineFile.AppendLine(@"            TypeResponse = TypeResponse.Warning;");
                lineFile.AppendLine(@"        }");
                lineFile.AppendLine(@"");
                lineFile.AppendLine(@"        public void Ok(T value, string message = """")");
                lineFile.AppendLine(@"        {");
                lineFile.AppendLine(@"            if (value != null)");
                lineFile.AppendLine(@"                Data = value;");
                lineFile.AppendLine(@"");
                lineFile.AppendLine(@"            Message = message;");
                lineFile.AppendLine(@"            Success = true;");
                lineFile.AppendLine(@"            TypeResponse = TypeResponse.Succes;");
                lineFile.AppendLine(@"        }");
                lineFile.AppendLine(@"");
                lineFile.AppendLine(@"        public void Ok(string message = """")");
                lineFile.AppendLine(@"        {");
                lineFile.AppendLine(@"            Message = message;");
                lineFile.AppendLine(@"            Success = true;");
                lineFile.AppendLine(@"            TypeResponse = TypeResponse.Succes;");
                lineFile.AppendLine(@"        }");
                lineFile.AppendLine(@"");
                lineFile.AppendLine(@"        public void Error(Exception ex, string errorMessage = """")");
                lineFile.AppendLine(@"        {");
                lineFile.AppendLine(@"            Message = $""{ex.Message}"";");
                lineFile.AppendLine(@"            if (!string.IsNullOrWhiteSpace(errorMessage))");
                lineFile.AppendLine(@"                Message += $""\n error: {errorMessage}"";");
                lineFile.AppendLine(@"");
                lineFile.AppendLine(@"            bool writeLog = bool.Parse(""writeLog"".ReadAppConfig(""false""));");
                lineFile.AppendLine(@"");
                lineFile.AppendLine(@"            if (writeLog)");
                lineFile.AppendLine(@"            {");
                lineFile.AppendLine(@"                if (!string.IsNullOrWhiteSpace(Message))");
                lineFile.AppendLine(@"                    Logger.ErrorFatal(ex, Message);");
                lineFile.AppendLine(@"                else");
                lineFile.AppendLine(@"                    Logger.ErrorFatal(ex);");
                lineFile.AppendLine(@"            }");
                lineFile.AppendLine(@"");
                lineFile.AppendLine(@"            Success = false;");
                lineFile.AppendLine(@"            TypeResponse = TypeResponse.Fatal;");
                lineFile.AppendLine(@"        }");
                lineFile.AppendLine(@"    }");
                lineFile.AppendLine(@"");
                if (isModelViewIsWCF)
                    lineFile.AppendLine(@"    [DataContract(Name = ""TypeResponse"")]");
                lineFile.AppendLine(@"    public enum TypeResponse");
                lineFile.AppendLine(@"    {");
                if (isModelViewIsWCF)
                    lineFile.AppendLine(@"        [EnumMember]");
                lineFile.AppendLine(@"        Na,");
                lineFile.AppendLine(@"");
                if (isModelViewIsWCF)
                    lineFile.AppendLine(@"        [EnumMember]");
                lineFile.AppendLine(@"        Fatal,");
                lineFile.AppendLine(@"");
                if (isModelViewIsWCF)
                    lineFile.AppendLine(@"        [EnumMember]");
                lineFile.AppendLine(@"        Succes,");
                lineFile.AppendLine(@"");
                if (isModelViewIsWCF)
                    lineFile.AppendLine(@"        [EnumMember]");
                lineFile.AppendLine(@"        Warning");
                lineFile.AppendLine(@"    }");


                return lineFile;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void CreateClassResponseModels()
        {
            try
            {
                var directoryProjectModelBase = Path.Combine("BaseDirectory".ReadAppConfig(), @"Services\Responses");
                var filePathBaseResponse = Path.Combine("BaseDirectory".ReadAppConfig(), @"Services\Responses\ResponseData.cs");
                var fileBaseResponse = CreateClassResponseBase();

                var extensionFile = ".cs";

                if (Directory.Exists(directoryProjectModelBase))
                    Directory.Delete(directoryProjectModelBase, true);

                Directory.CreateDirectory(directoryProjectModelBase);

                File.WriteAllText(filePathBaseResponse, fileBaseResponse.ToString());

                foreach (var table in tables)
                {
                    string file = Path.Combine(directoryProjectModelBase, $"{table.ClassNameModelResponse}{extensionFile}");

                    var line = new StringBuilder();
                    bool isModelViewIsWCF = bool.Parse("ModelViewIsWCF".ReadAppConfig());
                    line.AppendLine($"using {table.NameSpace}.Services.ModelView;");
                    line.AppendLine(@"using System.Collections.Generic;");
                    if (isModelViewIsWCF)
                        line.AppendLine(@"using System.Runtime.Serialization;");
                    line.AppendLine(@"");
                    line.AppendLine($"namespace {table.NameSpace}.Services.Responses");
                    line.AppendLine(@"{");
                    if (isModelViewIsWCF)
                        line.AppendLine($"\t[Serializable]");
                    if (isModelViewIsWCF)
                        line.AppendLine($"\t[DataContract(Name = \"{ table.ClassNameModelResponse}\")]");
                    line.AppendLine($"    public class {table.ClassNameModelResponse} : ResponseData<{table.ClassNameModelView}>");
                    line.AppendLine(@"    {");
                    line.AppendLine($"        public {table.ClassNameModelResponse}()");
                    line.AppendLine(@"        {");
                    line.AppendLine($"            Data = new {table.ClassNameModelView}();");
                    line.AppendLine(@"        }");
                    line.AppendLine(@"    }");
                    line.AppendLine(@"");
                    if (isModelViewIsWCF)
                        line.AppendLine($"\t[Serializable]");
                    if (isModelViewIsWCF)
                        line.AppendLine($"\t[DataContract(Name = \"{ table.ClassNameModelResponse}List\")]");
                    line.AppendLine($"    public class {table.ClassNameModelResponse}List : ResponseData<List<{table.ClassNameModelView}>>");
                    line.AppendLine(@"    {");
                    line.AppendLine($"        public {table.ClassNameModelResponse}List()");
                    line.AppendLine(@"        {");
                    line.AppendLine($"            Data = new List<{table.ClassNameModelView}>();");
                    line.AppendLine(@"        }");
                    line.AppendLine(@"    }");
                    line.AppendLine(@"}");

                    File.WriteAllText(file, line.ToString());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void CreateClassProfile()
        {
            try
            {
                var directoryProjectModelBase = Path.Combine("BaseDirectory".ReadAppConfig(), @"Services\Profiles");
                var nameSpace = "NameSpaceProject".ReadAppConfig();
                var extensionFile = ".cs";

                if (Directory.Exists(directoryProjectModelBase))
                    Directory.Delete(directoryProjectModelBase, true);

                Directory.CreateDirectory(directoryProjectModelBase);

                foreach (var table in tables)
                {
                    string file = Path.Combine(directoryProjectModelBase, $"{table.ClassNameModelProfile}{extensionFile}");

                    var line = new StringBuilder();
                    line.AppendLine(@"using AutoMapper;");
                    line.AppendLine($"using {nameSpace}.Models;");
                    line.AppendLine($"using {nameSpace}.Services.ModelView;");
                    line.AppendLine(@"");
                    line.AppendLine($"namespace {nameSpace}.Services.Profiles");
                    line.AppendLine(@"{");
                    line.AppendLine($"    public class {table.ClassNameModelProfile} : Profile");
                    line.AppendLine(@"    {");
                    line.AppendLine($"        public {table.ClassNameModelProfile}()");
                    line.AppendLine(@"        {");
                    line.AppendLine($"            CreateMap<{table.TableName}, {table.ClassNameModelView}>().");
                    foreach (var property in table.Properties)
                    {
                        line.AppendLine($"               ForMember(x => x.{property.ColumnName}, y => y.MapFrom(src => src.{property.ColumnName})).");
                    }

                    line.AppendLine(@"               ReverseMap();");
                    line.AppendLine(@"        }");
                    line.AppendLine(@"    }");
                    line.AppendLine(@"}");

                    File.WriteAllText(file, line.ToString());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void CreateClassServices()
        {
            try
            {
                var directoryProjectModelBase = Path.Combine("BaseDirectory".ReadAppConfig(), "Services");
                var nameSpace = "NameSpaceProject".ReadAppConfig();
                var extensionFile = ".cs";

                if (Directory.Exists(directoryProjectModelBase))
                    Directory.Delete(directoryProjectModelBase, true);

                Directory.CreateDirectory(directoryProjectModelBase);

                foreach (var table in tables)
                {
                    string file = Path.Combine(directoryProjectModelBase, $"{table.ClassNameServices}{extensionFile}");

                    var line = new StringBuilder();

                    line.AppendLine(@"using AutoMapper;");
                    line.AppendLine($"using {nameSpace}.Data;");
                    line.AppendLine($"using {nameSpace}.Services.ModelView;");
                    line.AppendLine($"using {nameSpace}.Services.Responses;");
                    line.AppendLine($"using {nameSpace}.Tools;");
                    line.AppendLine(@"using System;");
                    line.AppendLine(@"using System.Collections.Generic;");
                    line.AppendLine(@"");
                    line.AppendLine($"namespace {nameSpace}.Services");
                    line.AppendLine(@"{");
                    line.AppendLine($"    public class {table.ClassNameServices} : BaseServices, I{table.ClassNameServices}");
                    line.AppendLine(@"    {");
                    line.AppendLine($"        private readonly {table.ClassNameIRepository} _{table.ClassNameVariableRepository};");
                    line.AppendLine(@"");
                    line.AppendLine($"        public {table.ClassNameServices}({table.ClassNameIRepository} {table.ClassNameVariableRepository}, IMapper mapper) : base(mapper)");
                    line.AppendLine(@"        {");
                    line.AppendLine($"            _{table.ClassNameVariableRepository} = {table.ClassNameVariableRepository} ?? throw new ArgumentNullException(nameof({table.ClassNameVariableRepository}));");
                    line.AppendLine(@"        }");
                    line.AppendLine(@"");
                    line.AppendLine(@"    }");
                    line.AppendLine(@"");
                    line.AppendLine($"    public interface I{table.ClassNameServices}");
                    line.AppendLine(@"    {");
                    line.AppendLine(@"    }");
                    line.AppendLine(@"}");

                    File.WriteAllText(file, line.ToString());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}