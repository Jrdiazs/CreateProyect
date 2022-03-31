using System.Collections.Generic;
using System.Linq;

namespace CreateProject.Models
{
    public class TablesView
    {
        public string TableName { get; set; }

        public string PrimaryKey { get; set; }

        public string NameSpace { get; set; }

        public List<PropertiesView> Properties { get; set; }

        public string ClassNameRepository
        { get { return $"{TableName}Repository"; } }

        public string ClassNameIRepository
        { get { return $"I{TableName}Repository"; } }

        public string ClassNameModelView
        { get { return $"{TableName}ModelView"; } }

        public string ClassNameModelResponse
        { get { return $"{TableName}Response"; } }

        public string ClassNameModelProfile
        { get { return $"{TableName}Profile"; } }

        public string ClassNameServices
        { get { return $"{TableName}Services"; } }

        public string ClassNameVariableRepository
        {
            get
            {
                var firstLetter = ClassNameRepository.Substring(0, 1);
                var temp = $"{firstLetter.ToLower()}";
                foreach (var item in ClassNameRepository.ToCharArray().Skip(1))
                    temp += item;

                return temp;
            }
        }
    }
}