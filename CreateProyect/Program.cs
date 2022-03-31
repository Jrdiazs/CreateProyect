using CreateProject.Services;
using System;

namespace CreateProject
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var pr = new ProjectCreateClass();
                pr.CreateClassModels();//Create models table
                pr.CreateClassRepository();//Create class repository
                pr.CreateClassServices();
                pr.CreateClassServicesModels();//Create class repository
                pr.CreateClassResponseModels();
                pr.CreateClassProfile();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}