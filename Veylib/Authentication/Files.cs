using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Veylib.Authentication
{
    public class Files
    {
        public enum FileState
        {
            UnknownFile,
            GotFile,
            ServerError,
            UnknownErrr
        }

        public class FileData
        {
            public FileState State;
            public string ErrorMessage;
            
            public string FileName;
            public string FileContent;
            
            public int ApplicationId;
            public int OwnerId;
        }

        private static FileData jsonToFile(dynamic json)
        {
            throw new NotImplementedException();
            return new FileData { };
            //if (json.code == 200)
            //{
            //    var data = new FileData { State = FileState.GotFile, FileName =  };
            //    return data;
            //}
            //else if (json.code == 400)
            //{
            //    FileState err;
            //    switch (((string)json.extra).ToLower())
            //    {
            //        case "invalid credentials were provided.":
            //            err = FileState.InvalidCredentials;
            //            break;
            //        case "account is disabled.":
            //            err = FileState.AccountDisabled;
            //            break;
            //        case "application is disabled.":
            //            err = FileState.ApplicationDisabled;
            //            break;
            //        case "hardware id is invalid.":
            //            err = FileState.InvalidHWID;
            //            break;
            //        case "unknown user.":
            //            err = FileState.UnknownUser;
            //            break;
            //        case "username is already taken.":
            //            err = FileState.UsernameTaken;
            //            break;
            //        default:
            //            err = FileState.ServerError;
            //            break;
            //    }

            //    return new FileData { State = err, ErrorMessage = json.extra };
            //}
            //else
            //    return new FileData { State = UserVerificationState.UnknownError };
        }

        private static dynamic getRaw(int appId, int userId, string filename)
        {
            if (User.CurrentUser == null || User.CurrentUser.State != User.UserVerificationState.ValidCredentials) throw new User.NotLoggedIn();

            var req = Shared.GenerateWR($"auth/file/{appId}/{userId}/{filename}");
            var json = Shared.ReadResponse(req);

            return json;
        }

        public static FileData Get(int appId, int userId, string filename)
        {
            return jsonToFile(getRaw(appId, userId, filename).extra);
        }

        public static FileData Get(string filename)
        {
            return Get(Shared.AppID, User.CurrentUser.Id, filename);
        }

        public static List<FileData> GetAll(int appId, int userId)
        {
            var lof = new List<FileData>();

            var json = getRaw(appId, userId, "all");
            if (json == null)
                return lof;

            for (var x = 0; x < ((string)json.extra).Length; x++)
            {
                lof.Add(jsonToFile(json.extra[x]));
            }

            return lof;
        }

        public static List<FileData> GetAll(int userId)
        {
            return GetAll(Shared.AppID, userId);
        }

        public static List<FileData> GetAll()
        {
            return GetAll(Shared.AppID, User.CurrentUser.Id);
        }

        public static void Create()
        {

        }

        public static void Delete()
        {

        }
    }
}
