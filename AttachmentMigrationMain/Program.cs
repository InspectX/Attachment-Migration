using AttachmentMigrationMain;
using Microsoft.Extensions.Configuration;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Formats.Asn1;
using CsvHelper;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using System;
using CodeLab.Bravo.Utilities;
using System.Text.RegularExpressions;
using System.Linq;
using CsvHelper.Configuration;
using System.Reflection.PortableExecutable;

internal class Program
{
    public static Appsettings appsettings;
    private static void Main(string[] args)
    {

        var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

        IConfiguration config = builder.Build();

        appsettings = config.GetSection("Settings").Get<Appsettings>();


        Process();

        Console.ReadLine();
    }
    public static void Process()
    {
        string path, fileName, fileExtension, encryptedFileNameWithExtension;
        var records = ReadCSVFile();
        var tasks = GetTasksIdsAndReferenceNumber();
        var selfEvaluationTasks = GetSelfEvaluationTasksIdsAndReferenceNumber();
        var violationTasksTasks = GetViolationTaskIdsAndReferenceNumber();
        var appeals = GetAppealsIdsAndReferenceNumber();
        long tasksFinished = 1;
        foreach (var item in records)
        {

            try
            {
                (fileName, fileExtension) = GetAttchmentWithExtension(item.DOC_NAME);
                encryptedFileNameWithExtension = Crypto.EncryptAES(fileName + fileExtension);
                if (item.RECORD_ID.StartsWith("VN")) //violation
                {
                    var task = violationTasksTasks.FirstOrDefault(x => x.ReferenceNumber == item.RECORD_ID);
                    var file = CallWebService(item);
                    //var file = CallLocalService(item);
                    if (file != null && file.Length > 0)
                    {
                        path = appsettings.TasksPath + "\\" + (task == null ? "ViolationTasksNotExists" : (task?.Id.ToString() + "\\Violations"));
                        UpdateTaskAttachmentTable(task?.Id, encryptedFileNameWithExtension, task?.Id + "/" + GetValidName(encryptedFileNameWithExtension) + fileExtension, fileExtension);
                        CreateDirectoryIfNotExists(path);
                        SaveFileInPath(file, Path.Combine(path, GetValidName(encryptedFileNameWithExtension) + fileExtension));
                    }
                }
                else if (item.RECORD_ID.StartsWith("RFA")) //appeals
                {
                    var appeal = appeals.FirstOrDefault(x => x.ReferenceNumber == item.RECORD_ID);
                    var file = CallWebService(item);
                    //var file = CallLocalService(item);
                    if (file != null && file.Length > 0)
                    {
                        path = appsettings.AppealsPath + "\\" + (appeal == null ? "AppealNotExists" : appeal?.Id.ToString());
                        UpdateGrievanceAttachmentTable(appeal.Id, encryptedFileNameWithExtension, GetValidName(encryptedFileNameWithExtension) + fileExtension, fileExtension);
                        CreateDirectoryIfNotExists(path);
                        SaveFileInPath(file, Path.Combine(path, GetValidName(encryptedFileNameWithExtension) + fileExtension));
                    }
                }


                Console.WriteLine($"Downloading {Math.Round(((tasksFinished * 1.0 / records.Count) * 100), 4)}%");
                tasksFinished++;

            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error on getting attachments with RECORD_ID = {item.RECORD_ID}");
            }
        }
        Console.WriteLine($"Finished Downloading attachments");
    }



    public static byte[] CallLocalService(AttachmentViewDTO attachmentViewDTO)
    {
        HttpClient http = new HttpClient();

        var url = $"{attachmentViewDTO.URL}?token={attachmentViewDTO.TOKEN}&documentLength={attachmentViewDTO.FILE_SIZE}&documentID={attachmentViewDTO.DOCCODE}&fileName={attachmentViewDTO.DOC_NAME}";

        var responseResult = http.PostAsync($"http://176.105.149.192:7002/integration/EFAA/test", new StringContent(JsonConvert.SerializeObject(new { URL = url }), Encoding.UTF8, "application/json")).Result;

        var response = responseResult.Content.ReadAsStreamAsync().Result;

        var result = StramToByteArray(response);
        return result;
    }
    public static byte[] StramToByteArray(Stream stream)
    {
        byte[] bytes;
        List<byte> totalStream = new();
        byte[] buffer = new byte[32];
        int read;
        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            totalStream.AddRange(buffer.Take(read));
        }
        bytes = totalStream.ToArray();
        return bytes;
    }
    public static byte[] CallWebService(AttachmentViewDTO attachmentViewDTO)
    {
        HttpClient http = new HttpClient();

        var url = $"{attachmentViewDTO.URL}?token={attachmentViewDTO.TOKEN}&documentLength={attachmentViewDTO.FILE_SIZE}&documentID={attachmentViewDTO.DOCCODE}&fileName={attachmentViewDTO.DOC_NAME}";

        var responseResult = http.GetAsync(url).Result;

        var response = responseResult.Content.ReadAsStreamAsync().Result;

        var result = StramToByteArray(response);

        return result;
    }
    public static void CreateDirectoryIfNotExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
    public static void SaveFileInPath(byte[] file, string path)
    {
        using var writer = new BinaryWriter(File.OpenWrite(path));
        writer.Write(file);
    }
    public static string GetValidName(string name)//to remove invalid file or folder chars after hashing and return it fixed 5 chars length
    {
        string invalidPathChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
        Regex r = new Regex(string.Format("[{0}]", Regex.Escape(invalidPathChars)));
        string validName = r.Replace(name, "");

        return validName;
    }
    public static List<AttachmentViewDTO> ReadCSVFile()
    {

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
            BadDataFound = null,
            HasHeaderRecord = true,
            IgnoreBlankLines = false
        };
        using (var reader = new StreamReader(appsettings.MigrationAttachmentXSLPath))
        using (var csv = new CsvReader(reader, config))
        {
            var records = csv.GetRecords<AttachmentViewDTO>();
            return records.ToList();
        }
    }

    public static (string, string) GetAttchmentWithExtension(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return ("", "");
        }

        string name = Path.GetFileNameWithoutExtension(fileName);
        string extension = Path.GetExtension(fileName);
        return (name, extension);
    }
    public static List<QueryResult> GetTasksIdsAndReferenceNumber()
    {
        List<QueryResult> result;
        using (SqlConnection connection = new SqlConnection(appsettings.BravoConnectionString))
        {
            result = connection.Query<QueryResult>("SELECT Id,ReferenceNumber FROM TaskAssignment") as List<QueryResult>;
        }
        return result;
    }
    public static List<QueryResult> GetSelfEvaluationTasksIdsAndReferenceNumber()
    {
        List<QueryResult> result;
        using (SqlConnection connection = new SqlConnection(appsettings.BravoConnectionString))
        {
            result = connection.Query<QueryResult>("select Id,ReferenceNumber from SelfEvaluationTaskDetails") as List<QueryResult>;
        }
        return result;
    }

    public static List<QueryResult> GetViolationTaskIdsAndReferenceNumber()
    {
        List<QueryResult> result;
        using (SqlConnection connection = new SqlConnection(appsettings.BravoConnectionString))
        {
            result = connection.Query<QueryResult>(@"select Id,v.[RECORD_ID] as ReferenceNumber
                                                  from [dbo].[ViolationService_ASI] v
                                                  inner join TaskAssignment task
                                                  on v.INSPECTIONINFORMATION_SourceID = task.ReferenceNumber") as List<QueryResult>;
        }
        return result;
    }

    public static List<QueryResult> GetAppealsIdsAndReferenceNumber()
    {
        List<QueryResult> result;
        using (SqlConnection connection = new SqlConnection(appsettings.BravoConnectionString))
        {
            result = connection.Query<QueryResult>(@"select Id,ReferenceNumber from EntityPortal.Grievances") as List<QueryResult>;
        }
        return result;
    }

    private static void UpdateGrievanceAttachmentTable(long grievanceId, string encryptedFileName, string path, string extensoin)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(appsettings.BravoConnectionString))
            {
                dynamic result = connection.Query(@$"select g.TaskAssignmentId TaskId,g.Id GrievanceId  from EntityPortal.Grievances g
where g.ID = {grievanceId}").FirstOrDefault();
                if (result.TaskId != null)
                {
                    string query = $"insert into EntityPortal.GrievanceAttachments ([TaskId],[GrievanceId],[FileName],[FilePath],[FileExtension],[AttachmentDate],[IncidentId]) Values ({result.TaskId},{result.GrievanceId},N'{encryptedFileName}',N'{path}','{extensoin}',GetDate(),NULL)";
                    connection.Execute(query);
                }

            }
        }
        catch (Exception ex)
        {

            Console.WriteLine(ex.Message);
        }
    }

    private static void UpdateTaskAttachmentTable(long? taskId, string encryptedFileName, string path, string extensoin)
    {
        if (taskId == null) return;
        try
        {
            using (SqlConnection connection = new SqlConnection(appsettings.BravoConnectionString))
            {

                string query = $"insert into SubmissionAttachments ([TaskId],[FileName],[FilePath],[FileExtension],[AttachmentDate]) Values ({taskId},N'{encryptedFileName}',N'{path}','{extensoin}',GetDate())";
                connection.Execute(query);


            }
        }
        catch (Exception ex)
        {

            Console.WriteLine(ex.Message);
        }
    }
}