﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using RestSharp;

namespace SynologyClient
{
    public class SynologyApi : ISynologyApi
    {
        public enum FileSystemType
        {
            none,
            cifs,
            iso
        }

        public enum ThumbnailRotateOptions
        {
            none,
            rotate90,
            rotate180,
            rotate270,
            rotate360
        }

        public enum ThumbnailSizeOption
        {
            small,
            medium,
            large,
            original
        }

        public enum download_mode
        {
            open,
            download
        }

        public enum filetype
        {
            file,
            dir,
            all
        }


        public enum sharing_sort_by
        {
            id,
            isFolder,
            path,
            date_expired,
            date_available,
            status,
            has_password,
            url,
            link_owner
        }

        public enum sort_by
        {
            name,
            user,
            group,
            mtime,
            atime,
            ctime,
            posix,
            size
        }

        public enum sort_direction
        {
            asc,
            desc
        }

        public enum status_filter
        {
            valid,
            broken,
            all
        }

        private readonly IRestClient _client;
        private readonly ISynologyClientConfig _config;
        private readonly ISynologySession _session;

        public SynologyApi(ISynologyClientConfig config, ISynologySession session, IRestClient client = null)
        {
            if (config == null)
                throw new ArgumentNullException("config");
            if (session == null)
                throw new ArgumentNullException("session");
            _config = config;
            _session = session;

            if (string.IsNullOrEmpty(_session.sid))
                throw new SynologyClientException("Session Id is empty");

            _client = client;
        }

        public SynologyResponse SynoFilestationInfo()
        {
            var proc = new FuncProcessor("/FileStation/info.cgi", _session.sid, new
            {
                api = "SYNO.FileStation.Info",
                version = 1,
                method = "getinfo"
            });

            return proc.Run();
        }

        public SynologyResponse SynoFileStationListShare(int? offset = null, int? limit = null,
            sort_by sort_by = sort_by.ctime, sort_direction sort_direction = sort_direction.asc,
            bool onlywritable = false, FileListAddtionalOptions additional = null)
        {
            dynamic requiredParams = new
            {
                api = "SYNO.FileStation.List",
                version = 1,
                method = "list_share",
                offset,
                limit,
                sort_by,
                sort_direction,
                onlywritable
            };

            var proc = new FuncProcessor("/FileStation/file_share.cgi", _session.sid, requiredParams, new
            {
                additional = TypeBooleanValuesToCommaSeparated(additional)
            });

            return proc.Run();
        }

        public SynologyResponse SynoFileStationList(string folderPath, int? offset = null, int? limit = null,
            sort_by sortBy = sort_by.ctime, sort_direction sortDirection = sort_direction.asc, string pattern = null,
            filetype fileType = filetype.all, string goto_path = null, FileListAddtionalOptions additional = null)
        {
            dynamic requiredParams = new
            {
                api = "SYNO.FileStation.List",
                version = 1,
                method = "list",
                folder_path = folderPath,
                offset,
                limit,
                sort_by = sortBy,
                sort_direction = sortDirection,
                pattern,
                filetype = fileType,
                goto_path
            };

            var proc = new FuncProcessor("/FileStation/file_share.cgi", _session.sid, requiredParams, new
            {
                additional = TypeBooleanValuesToCommaSeparated(additional)
            });

            return proc.Run();
        }

        public SynologyResponse SynoFileStationListGetInfo(string[] paths, FileGetInfoAddtionalOptions additional = null)
        {
            dynamic requiredParams = new
            {
                api = "SYNO.FileStation.List",
                version = 1,
                method = "getinfo",
                path = string.Join(",", paths)
            };

            var proc = new FuncProcessor("/FileStation/file_share.cgi", _session.sid, requiredParams, new
            {
                additional = TypeBooleanValuesToCommaSeparated(additional)
            });

            return proc.Run();
        }

        /// webapi/FileStation/file_find.cgi?api=SYNO.FileStation.Search&version=1&method=start&folder_path=%2Fvideo&pattern=1
        public SynologyResponse SynoFileStationSearchStart(string folderPath, bool recursive = true,
            string[] globPatterns = null, string[] extentionPatterns = null, filetype fileType = filetype.file,
            long minSizeBytes = 0, long maxSizeBytes = Int64.MaxValue, DateTime? modifiedTimeFrom = null,
            DateTime? modifiedTimeTo = null, DateTime? createdTimeFrom = null, DateTime? createdTimeTo = null,
            DateTime? accessedTimeTo = null, DateTime? accessedTimeFrom = null, string owner = null, string group = null)
        {
            dynamic requiredParams = new
            {
                api = "SYNO.FileStation.Search",
                version = 1,
                method = "start",
                folder_path = folderPath,
                recursive,
                pattern = string.Join(",", globPatterns ?? new[] {""}),
                extension = string.Join(",", extentionPatterns ?? new[] {""}),
                filetype = fileType,
                size_from = minSizeBytes,
                size_to = maxSizeBytes,
                mtime_from = modifiedTimeFrom,
                mtime_to = modifiedTimeTo,
                crtime_from = createdTimeFrom,
                crtime_to = createdTimeTo,
                atime_from = accessedTimeFrom,
                atiime_to = accessedTimeTo,
                owner,
                group
            };

            var proc = new FuncProcessor("/FileStation/file_find.cgi", _session.sid, requiredParams);

            return proc.Run();
        }

        public SynologyResponse SynoFileStationSearchList(string taskId, int? offset = null, int? limit = 100,
            sort_by sortBy = sort_by.name, sort_direction sortDirection = sort_direction.asc, string[] pattern = null,
            filetype fileType = filetype.file, FileSearchListAddtionalOptions additional = null)
        {
            dynamic requiredParams = new
            {
                api = "SYNO.FileStation.Search",
                version = 1,
                method = "list",
                taskid = taskId,
                offset,
                limit,
                sort_by = sortBy,
                sort_direction = sortDirection,
                pattern = string.Join(",", pattern ?? new[] {""}),
                filetype = fileType
            };

            var proc = new FuncProcessor("/FileStation/file_find.cgi", _session.sid, requiredParams, new
            {
                additional = TypeBooleanValuesToCommaSeparated(additional)
            });

            return proc.Run();
        }

        public SynologyResponse SynoFileStationSearchStop(string taskId)
        {
            dynamic requiredParams = new
            {
                api = "SYNO.FileStation.Search",
                version = 1,
                method = "stop",
                taskid = taskId
            };

            var proc = new FuncProcessor("/FileStation/file_find.cgi", _session.sid, requiredParams);
            return proc.Run();
        }

        public SynologyResponse SynoFileStationSearchClean(string taskId)
        {
            dynamic requiredParams = new
            {
                api = "SYNO.FileStation.Search",
                version = 1,
                method = "clean",
                taskid = taskId
            };

            var proc = new FuncProcessor("/FileStation/file_find.cgi", _session.sid, requiredParams);
            return proc.Run();
        }

        public SynologyResponse SynoFileStationVirtualFolderList(FileSystemType fileSystemType = FileSystemType.cifs,
            int? offset = null, int? limit = null, sort_by sort_by = sort_by.ctime,
            sort_direction sort_direction = sort_direction.asc, VirtualFolderListAddtionalOptions additional = null)
        {
            dynamic requiredParams = new
            {
                api = "SYNO.FileStation.VirtualFolder",
                version = 1,
                method = "list",
                type = fileSystemType,
                offset,
                limit,
                sort_by,
                sort_direction
            };

            var proc = new FuncProcessor("/FileStation/file_virtual.cgi", _session.sid, requiredParams, new
            {
                additional = TypeBooleanValuesToCommaSeparated(additional)
            });
            return proc.Run();
        }

        public SynologyResponse SynoFileStationFavoriteList(int? offset = null, int? limit = null,
            status_filter statusFilter = status_filter.all, FileStationFavoriteAddtionalOptions additional = null)
        {
            dynamic requiredParams = new
            {
                api = "SYNO.FileStation.Favorite",
                version = 1,
                method = "list",
                offset,
                limit,
                status_filter = statusFilter
            };

            var proc = new FuncProcessor("/FileStation/file_favorite.cgi", _session.sid, requiredParams, new
            {
                additional = TypeBooleanValuesToCommaSeparated(additional)
            });
            return proc.Run();
        }

        public SynologyResponse SynoFileStationFavoriteAdd(string path, string name, int index = -1)
        {
            dynamic requiredParams = new
            {
                api = "SYNO.FileStation.Favorite",
                version = 1,
                method = "add",
                path,
                name
            };

            var proc = new FuncProcessor("/FileStation/file_favorite.cgi", _session.sid, requiredParams);
            return proc.Run();
        }

        public SynologyResponse SynoFileStationFavoriteDelete(string path)
        {
            dynamic requiredParams = new
            {
                api = "SYNO.FileStation.Favorite",
                version = 1,
                method = "delete",
                path
            };

            var proc = new FuncProcessor("/FileStation/file_favorite.cgi", _session.sid, requiredParams);
            return proc.Run();
        }

        public SynologyResponse SynoFileStationFavoriteClearBroken(string path, string name)
        {
            dynamic requiredParams = new
            {
                api = "SYNO.FileStation.Favorite",
                version = 1,
                method = "clear_broken",
                path,
                name
            };

            var proc = new FuncProcessor("/FileStation/file_favorite.cgi", _session.sid, requiredParams);
            return proc.Run();
        }

        public SynologyResponse SynoFileStationFavoriteReplaceAll(string path, string name)
        {
            dynamic requiredParams = new
            {
                api = "SYNO.FileStation.Favorite",
                version = 1,
                method = "replace_all",
                path,
                name
            };

            var proc = new FuncProcessor("/FileStation/file_favorite.cgi", _session.sid, requiredParams);
            return proc.Run();
        }

        public byte[] SynoFileStationThumbGet(string path, ThumbnailSizeOption size = ThumbnailSizeOption.small,
            ThumbnailRotateOptions rotate = ThumbnailRotateOptions.none)
        {
            var request = new SynoRestRequest();
            request.AddParameter("api", "SYNO.FileStation.Thumb");
            request.AddParameter("version", "1");
            request.AddParameter("method", "get");
            request.AddParameter("path", path);
            request.AddParameter("size", size);
            request.AddParameter("rotate", rotate);
            request.AddParameter("_sid", _session.sid);
            var config = new SynologyClientConfig();
            var client = new RestClient(config.ApiBaseAddressAndPathNoTrailingSlash + "/FileStation/file_thumb.cgi");
            IRestResponse response = client.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new SynologyClientException("Errored with http status code " + response.StatusCode);
            return response.RawBytes;
        }

        public SynologyResponse SynoFileStationDirsizeStart(string path)
        {
            dynamic requiredParams = new
            {
                api = "SYNO.FileStation.DirSize",
                version = 1,
                method = "start",
                path
            };

            var proc = new FuncProcessor("/FileStation/file_dirSize.cgi", _session.sid, requiredParams);
            return proc.Run();
        }

        public SynologyResponse SynoFileStationDirsizeStatus(string taskId)
        {
            dynamic requiredParams = new
            {
                api = "SYNO.FileStation.DirSize",
                version = 1,
                method = "status",
                taskid = taskId
            };

            var proc = new FuncProcessor("/FileStation/file_dirSize.cgi", _session.sid, requiredParams);
            return proc.Run();
        }

        public SynologyResponse SynoFileStationDirsizeStop(string taskId)
        {
            dynamic requiredParams = new
            {
                api = "SYNO.FileStation.DirSize",
                version = 1,
                method = "stop",
                taskid = taskId
            };

            var proc = new FuncProcessor("/FileStation/file_dirSize.cgi", _session.sid, requiredParams);
            return proc.Run();
        }

        public SynologyResponse SynoFileStationMd5Start(string filePath)
        {
            dynamic requiredParams = new
            {
                api = "SYNO.FileStation.MD5",
                version = 1,
                method = "start",
                file_path = filePath
            };

            var proc = new FuncProcessor("/FileStation/file_md5.cgi", _session.sid, requiredParams);
            return proc.Run();
        }

        public SynologyResponse SynoFileStationMd5Status(string taskId)
        {
            dynamic requiredParams = new
            {
                api = "SYNO.FileStation.MD5",
                version = 1,
                method = "status",
                taskid = taskId
            };

            var proc = new FuncProcessor("/FileStation/file_md5.cgi", _session.sid, requiredParams);
            return proc.Run();
        }

        public SynologyResponse SynoFileStationMd5Stop(string taskId)
        {
            dynamic requiredParams = new
            {
                api = "SYNO.FileStation.MD5",
                version = 1,
                method = "stop",
                taskid = taskId
            };

            var proc = new FuncProcessor("/FileStation/file_md5.cgi", _session.sid, requiredParams);
            return proc.Run();
        }

        public SynologyResponse SynoFileStationCheckPermission(string path, bool? createOnly = true)
        {
            dynamic requiredParams = new
            {
                api = "SYNO.FileStation.CheckPermission",
                version = 1,
                method = "write",
                path,
                create_only = createOnly
            };

            var proc = new FuncProcessor("/FileStation/file_permission.cgi", _session.sid, requiredParams);
            return proc.Run();
        }

        public SynologyResponse SynoFileStationUpload(FileInfo fileName, string destinationFilePath,
            bool createParents = true, bool? overwrite = false)
        {
            var request = new SynoRestRequest(Method.POST);

            request.AddParameter("_sid", _session.sid);
            request.AddParameter("api", "SYNO.FileStation.Upload");
            request.AddParameter("version", "1");
            request.AddParameter("method", "upload");
            request.AddParameter("dest_folder_path", destinationFilePath);
            request.AddParameter("create_parents", createParents);
            request.AddParameter("mtime", DateTimeExtender.GetUnixTimeFromDate(fileName.LastWriteTimeUtc).ToString());
            request.AddParameter("crtime", DateTimeExtender.GetUnixTimeFromDate(fileName.CreationTimeUtc).ToString());
            request.AddParameter("atime", DateTimeExtender.GetUnixTimeFromDate(fileName.LastAccessTimeUtc).ToString());
            request.AddParameter("overwrite", overwrite);

            request.AddFile(fileName.Name, fileName.FullName);

            var config = new SynologyClientConfig();
            var client = new RestClient(config.ApiBaseAddressAndPathNoTrailingSlash + "/FileStation/api_upload.cgi");

            IRestResponse<SynologyResponse> response = client.Execute<SynologyResponse>(request);
            return response.Data;
        }

        public byte[] SynoFileStationDownload(string filePath, download_mode mode = download_mode.download)
        {
            var request = new SynoRestRequest();

            request.AddParameter("api", "SYNO.FileStation.Download");
            request.AddParameter("version", "1");
            request.AddParameter("method", "download");
            request.AddParameter("path", filePath);
            request.AddParameter("mode", mode);
            request.AddParameter("_sid", _session.sid);
            var config = new SynologyClientConfig();
            var client = new RestClient(config.ApiBaseAddressAndPathNoTrailingSlash + "/FileStation/file_download.cgi");
            IRestResponse response = client.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new SynologyClientException("Errored with http status code " + response.StatusCode);
            return response.RawBytes;
        }

        public SynologyResponse SynoFileStationSharingGetInfo(string id)
        {
            dynamic requiredParams = new
            {
                api = "SYNO.FileStation.Sharing",
                version = 1,
                method = "getinfo",
                id
            };

            var proc = new FuncProcessor("/FileStation/file_sharing.cgi", _session.sid, requiredParams);
            return proc.Run();
        }

        public SynologyResponse SynoFileStationSharingList(int? offset, int? limit, sharing_sort_by sortBy,
            sort_direction sortDirection = sort_direction.asc, bool? forceClean = true)
        {
            dynamic requiredParams = new
            {
                api = "SYNO.FileStation.Sharing",
                version = 1,
                method = "list",
                offset,
                limit,
                sort_by = sortBy,
                sort_direction = sortDirection,
                force_clean = forceClean
            };

            var proc = new FuncProcessor("/FileStation/file_sharing.cgi", _session.sid, requiredParams);
            return proc.Run();
        }

        public SynologyResponse SynoFileStationSharingCreate(string path, string password = null, DateTime? dateExpires = null,
            DateTime? dateAvailable = null)
        {
            dynamic requiredParams = new
            {
                api = "SYNO.FileStation.Sharing",
                version = 1,
                method = "create",
                path,
                password,
                date_expired = dateExpires.HasValue ? dateExpires.Value.ToString("yyyy-MM-dd") : "0",
                date_available = dateAvailable.HasValue ? dateAvailable.Value.ToString("yyyy-MM-dd") : "0"
            };

            var proc = new FuncProcessor("/FileStation/file_sharing.cgi", _session.sid, requiredParams);
            return proc.Run();
        }

        public SynologyResponse SynoFileStationSharingDelete(string id)
        {
            dynamic requiredParams = new
            {
                api = "SYNO.FileStation.Sharing",
                version = 1,
                method = "delete",
                id
            };

            var proc = new FuncProcessor("/FileStation/file_sharing.cgi", _session.sid, requiredParams);
            return proc.Run();
        }

        public SynologyResponse SynoFileStationSharingClearInvalid(string id)
        {
            dynamic requiredParams = new
            {
                api = "SYNO.FileStation.Sharing",
                version = 1,
                method = "clear_invalid",
                id
            };

            var proc = new FuncProcessor("/FileStation/file_sharing.cgi", _session.sid, requiredParams);
            return proc.Run();
        }

        public SynologyResponse SynoFileStationSharingEdit(string id, string password = null, DateTime? dateExpires = null, DateTime? dateAvailable = null)
        {
            dynamic requiredParams = new
            {
                api = "SYNO.FileStation.Sharing",
                version = 1,
                method = "edit",
                id,
                password,
                date_expired = dateExpires.HasValue ? dateExpires.Value.ToString("yyyy-MM-dd") : "0",
                date_available = dateAvailable.HasValue ? dateAvailable.Value.ToString("yyyy-MM-dd") : "0"
            };

            var proc = new FuncProcessor("/FileStation/file_sharing.cgi", _session.sid, requiredParams);
            return proc.Run();
        }

        public SynologyResponse SynoFileStationCreateFolder(string folderPath, string name, bool? forceParent, FileSearchListAddtionalOptions additional = null)
        {
            dynamic requiredParams = new
            {
                api = "SYNO.FileStation.CreateFolder",
                version = 1,
                method = "create",
                folder_path = folderPath,
                name,
                force_parent = forceParent
            };

            var proc = new FuncProcessor("/FileStation/file_favorite.cgi", _session.sid, requiredParams, new
            {
                additional = TypeBooleanValuesToCommaSeparated(additional)
            });
            return proc.Run();
        }

        // creates comma delimited list of only boolean public property names set as true
        private string TypeBooleanValuesToCommaSeparated<T>(T instance) where T : class
        {
            if (instance == null)
                return null;

            string[] selected = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => (bool)p.GetValue(instance, null))
                .Select(p => p.Name).ToArray();

            return selected.Any() ? string.Join(",", selected) : null;
        }

        public SynologyResponse SynoFileStationFavoriteEdit(string path, string name)
        {
            dynamic requiredParams = new
            {
                api = "SYNO.FileStation.Favorite",
                version = 1,
                method = "edit",
                path,
                name
            };

            var proc = new FuncProcessor("/FileStation/file_favorite.cgi", _session.sid, requiredParams);
            return proc.Run();
        }

        public class FileGetInfoAddtionalOptions
        {
            public bool real_path { get; set; }

            public bool size { get; set; }

            public bool owner { get; set; }

            public bool time { get; set; }

            public bool perm { get; set; }

            public bool mount_point_type { get; set; }

            public bool type { get; set; }
        }

        public class FileListAddtionalOptions
        {
            public bool real_path { get; set; }

            public bool size { get; set; }

            public bool owner { get; set; }

            public bool time { get; set; }

            public bool perm { get; set; }

            public bool mount_point_type { get; set; }

            public bool volume_status { get; set; }
        }

        public class FileSearchListAddtionalOptions
        {
            public bool real_path { get; set; }

            public bool size { get; set; }

            public bool owner { get; set; }

            public bool time { get; set; }

            public bool perm { get; set; }

            public bool type { get; set; }
        }

        public class FileStationFavoriteAddtionalOptions
        {
            public bool real_path { get; set; }

            public bool size { get; set; }

            public bool owner { get; set; }

            public bool time { get; set; }

            public bool perm { get; set; }

            public bool mount_point_type { get; set; }
        }

        public class VirtualFolderListAddtionalOptions
        {
            public bool real_path { get; set; }

            public bool size { get; set; }

            public bool owner { get; set; }

            public bool time { get; set; }

            public bool perm { get; set; }

            public bool volume_status { get; set; }
        }
    }
}