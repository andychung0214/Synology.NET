﻿using System;
using System.IO;
using System.Reflection;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace SynologyClient.ApiTests
{
    [TestFixture]
    public class ApiTests
    {
        [SetUp]
        public void setup()
        {
            _session = new SynologySession(new SynologyClientConfig());
            _session.Login();
            _api = new SynologyApi(new SynologyClientConfig(), _session);
        }

        [TearDown]
        public void TearDown()
        {
            if (_session != null)
                _session.LogOut();
        }

        public ApiTests()
        {
            var executingAssembly = new FileInfo(Assembly.GetExecutingAssembly().FullName);
            _localTestFolderNoSlash = executingAssembly.DirectoryName + "\\TestFiles";
            _localTestImage = _localTestFolderNoSlash +  "\\image\\synologybox.jpg";
        }

        private readonly string _synoTestFolderNoSlash = "/public/apitest";
        private static string _localTestFolderNoSlash;
        private readonly string _localTestImage;

        private SynologySession _session;
        private SynologyApi _api;

        public ApiTests(string synoTestFolderNoSlash)
        {
            _synoTestFolderNoSlash = synoTestFolderNoSlash;
        }

        // [Test]
        public void SynoFilestationInfo()
        {
            SynologyResponse res = _api.SynoFilestationInfo();
            res.success.Should().BeTrue();
        }

        //[Test]
        public void FileStationListShare()
        {
            SynologyResponse res = _api.SynoFileStationListShare();
            res.success.Should().BeTrue();

            SynologyResponse res2 = _api.SynoFileStationListShare(
                0,
                0,
                SynologyApi.sort_by.name,
                SynologyApi.sort_direction.asc, false,
                new SynologyApi.FileListAddtionalOptions
                {
                    size = true,
                    time = true,
                    real_path = true
                });

            res2.success.Should().BeTrue();

            var atime = (int) (res2.data["shares"][0]["additional"]["time"]["atime"]);
            atime.Should().BeGreaterThan(0);

            string real_path = res2.data["shares"][0]["additional"]["real_path"];
            real_path.Should().NotBeNullOrEmpty();
        }

        //[Test]
        public void FileStationList()
        {
            SynologyResponse res = _api.SynoFileStationList("/Data");
            res.success.Should().BeTrue();

            SynologyResponse res2 = _api.SynoFileStationList("/Data", 0, 10, SynologyApi.sort_by.name,
                SynologyApi.sort_direction.asc, "*", SynologyApi.filetype.all, null,
                new SynologyApi.FileListAddtionalOptions
                {
                    real_path = true,
                    time = true
                });

            res2.success.Should().BeTrue();

            var atime = (int) (res2.data["files"][0]["additional"]["time"]["atime"]);
            atime.Should().BeGreaterThan(0);

            string real_path = res2.data["files"][0]["path"];
            real_path.Should().NotBeNullOrEmpty();
        }

        //[Test]
        public void FileStationGetInfo()
        {
            SynologyResponse res = _api.SynoFileStationListGetInfo(new[] {"/Data"},
                new SynologyApi.FileGetInfoAddtionalOptions {real_path = true, size = true, owner = true, time = true});
            res.success.Should().BeTrue();
        }

        //[Test]
        public void FileStationSearchStart()
        {
            SynologyResponse res = _api.SynoFileStationSearchStart("/Data");

            res.success.Should().BeTrue();

            string taskid = res.data["taskid"];
            taskid.Should().NotBeNullOrEmpty();

            bool finished = false;
            for (int i = 0; i < 10; i++)
            {
                SynologyResponse list = _api.SynoFileStationSearchList(taskid);
                list.success.Should().BeTrue();
                finished = (bool) list.data["finished"];
                if (finished)
                {
                    ((int) list.data["finished"]).Should().BeGreaterThan(0);
                    break;
                }

                Thread.Sleep(2000);
            }

            _api.SynoFileStationSearchStop(taskid).success.Should().BeTrue();
            _api.SynoFileStationSearchClean(taskid).success.Should().BeTrue();
        }

        // [Test]
        public void SynoFileStationVirtualFolderList()
        {
            SynologyResponse res = _api.SynoFileStationVirtualFolderList();
            res.success.Should().BeTrue();
        }

        // [Test]
        public void SynoFileStationFavoriteList()
        {
            SynologyResponse res = _api.SynoFileStationFavoriteList();
            res.success.Should().BeTrue();
        }

        //[Test]
        public void SynoFileStationFavoriteDelete()
        {
            SynologyResponse res = _api.SynoFileStationFavoriteDelete("/Data");
            res.success.Should().BeTrue();
        }

        //[Test]
        public void SynoFileStationFavoriteAdd()
        {
            try
            {
                SynologyResponse res = _api.SynoFileStationFavoriteAdd("/Data", "DataFavorite");
                res.success.Should().BeTrue();
            }
            catch (Exception e)
            {
                e.Message.Should().Be("A folder path of favorite folder is already added to user’s favorites");
            }
        }

        //[Test]
        public void SynoFileStationFavoriteClearBroken()
        {
            SynologyResponse res = _api.SynoFileStationFavoriteClearBroken("/Data", "DataFavorite");
            res.success.Should().BeTrue();
        }

        //[Test]
        public void SynoFileStationFavoriteEdit()
        {
            SynologyResponse res = _api.SynoFileStationFavoriteEdit("/Data", "DataFavorite");
            res.success.Should().BeTrue();
        }

        //[Test]
        public void SynoFileStationFavoriteReplaceAll()
        {
            SynologyResponse res = _api.SynoFileStationFavoriteReplaceAll("/Data", "DataFavorite");
            res.success.Should().BeTrue();
        }

        //[Test]
        public void SynoFileStationThumbGet()
        {
            byte[] res = _api.SynoFileStationThumbGet("/homes/allanb/20130524059.jpg");
            //File.WriteAllBytes("c:\\test\\20130524059.jpg", res);
            res.Length.Should().BeGreaterThan(1);
        }

        // [Test]
        public void SynoFileStationDirSize()
        {
            SynologyResponse res = _api.SynoFileStationDirsizeStart("/photo");

            res.success.Should().BeTrue();

            string taskid = res.data["taskid"];
            taskid.Should().NotBeNullOrEmpty();

            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(2000);
                SynologyResponse list = _api.SynoFileStationDirsizeStatus(taskid);
                list.success.Should().BeTrue();
                var finished = (bool) list.data["finished"];
                if (finished)
                {
                    ((bool) list.data["finished"]).Should().BeTrue();
                    break;
                }
            }

            _api.SynoFileStationDirsizeStatus(taskid).success.Should().BeTrue();
        }

        //[Test]
        public void SynoFileStationMd5()
        {
            SynologyResponse res = _api.SynoFileStationMd5Start("/homes/allanb/20130524059.jpg");

            res.success.Should().BeTrue();

            string taskid = res.data["taskid"];
            taskid.Should().NotBeNullOrEmpty();

            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(2000);
                SynologyResponse list = _api.SynoFileStationMd5Status(taskid);
                list.success.Should().BeTrue();
                var finished = (bool) list.data["finished"];
                if (finished)
                {
                    ((bool) list.data["finished"]).Should().BeTrue();
                    break;
                }
            }

            _api.SynoFileStationDirsizeStop(taskid).success.Should().BeTrue();
        }

        //[Test]
        public void SynoFileStationCheckPermission()
        {
            try
            {
                SynologyResponse res = _api.SynoFileStationCheckPermission("/Data");
                res.success.Should().BeTrue();
            }
            catch (Exception)
            {
                Assert.Pass();
            }
        }

        //[Test]
        public void SynoFileStationUpload_Download()
        {
            var local = new FileInfo(_localTestImage);
            string dest = _synoTestFolderNoSlash;
            string downloadedName = _localTestFolderNoSlash + "\\dload_" + local.Name;

            if (File.Exists(downloadedName))
                File.Delete(downloadedName);

            SynologyResponse res = _api.SynoFileStationUpload(new FileInfo(_localTestImage), dest, true, true);
            res.success.Should().BeTrue();

            Thread.Sleep(3000);

            byte[] file = _api.SynoFileStationDownload(dest + "/" + local.Name);
            file.Length.Should().Be((int) local.Length);
        }

        //[Test]
        public void SynoFileStationSharing()
        {
            var create = _api.SynoFileStationSharingCreate(_synoTestFolderNoSlash);
            var id = create.data["links"][0]["id"];
            create.success.Should().BeTrue();

            SynologyResponse info = _api.SynoFileStationSharingGetInfo(id);
            info.success.Should().BeTrue();

            SynologyResponse edit = _api.SynoFileStationSharingEdit(id);
            edit.success.Should().BeTrue();

            _api.SynoFileStationSharingDelete(id);

            _api.SynoFileStationSharingClearInvalid(id);

        }
    }
}