using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using AccreditationMS.Controllers;
using AccreditationMS.Core.PhotoServie;
using AccreditationMS.Core.UnitOfWork;
using AccreditationMS.Infrastructure;
using AccreditationMS.Infrastructure.Extensions;
using AccreditationMS.Models.Domain;
using AccreditationMS.Models.Views;
using demoIdentity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AccreditationMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("[controller]/[action]/{id?}")]
    [Authorize(Roles = "Admin,SuperAdmin,LOC,UEG,MediaManager,Observer")]
    [SessionAuthorize]
    public class MediaManageController : Controller
    {
        private IUnitOfWork _context;
        private IHostingEnvironment _hostingEnvironment;
        private IPhotoService _photoService;
        private IEmailSender  _emailSender;
        public MediaManageController(IUnitOfWork unitOfWork, IHostingEnvironment hostingEnviroment, IPhotoService photoService,IEmailSender emailSender)
        {
            _context = unitOfWork;
            _hostingEnvironment = hostingEnviroment;
            _photoService = photoService;
            _emailSender = emailSender;
        }
        private bool equalsContentType(string Orginal, params string[] Others)
        {
            foreach (var item in Others)
            {
                if (item.Equals(Orginal) == true)
                    return true;
            }
            return false;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [TypeFilter(typeof(EventToViewAttribute))]
        public async Task<IActionResult> Delete(int id)
        {
            var mediaDelegation = await _context.MediaDelegations.GetWithRelatedDataByIdAsync(id);
            if (mediaDelegation != null)
            {
                try
                {

                    int eventId = ViewBag.EventId;
                    string path = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", eventId.ToString(), "media");
                    string fullPathDeletePhoto = Path.Combine(path, mediaDelegation.PhotoPath);

                    bool isDeleted = _photoService.TryDelete(path, fullPathDeletePhoto);
                    if (!String.IsNullOrEmpty(mediaDelegation.PressCardPath))
                    {
                        string fullPathDeletePress = Path.Combine(path, mediaDelegation.PressCardPath);

                        bool isDeletedPressCard = _photoService.TryDelete(path, fullPathDeletePress);
                    }
                    _context.MediaDelegations.Remove(mediaDelegation);
                    if (isDeleted)
                    {
                       // await this.LogForDeleteAsync<MediaDelegation>(_context.LiveLogs, mediaDelegation, "Media Delegation");
                        await _context.SaveAsync();
                    }

                }
                catch (Exception exp)
                {
                    var m = exp.Message;
                }
                await _context.SaveAsync();

            }

            return RedirectToAction(nameof(Delegations));
        }


        [HttpPost]
        [SessionAuthorize]
        [TypeFilter(typeof(EventToViewAttribute))]
        public async Task<IActionResult> GetSubCategories([FromBody]MediaCategoryModel mediaCategory)
        {
            List<MediaSubCategory> mediaSubgroups = new List<MediaSubCategory>();
            if (ModelState.IsValid)
            {
                int eventId = (int)ViewBag.EventId;

                mediaSubgroups = (await _context.MediaSubCategories.GetByMediaCategoryIdAsync(mediaCategory.MediacategoryId)).ToList();
            }
            return Json(mediaSubgroups);
        }


        [ImportModelState]
        [TypeFilter(typeof(EventToViewAttribute))]
     
        public async Task<IActionResult> Delegations()
        { 
            Event currentEvent = ViewBag.CurrentEvent;
            
            //prepare models
            IEnumerable<AdminMediaDelegation> mediaDelegations = await _context.MediaDelegations.
                                                                         GetDelegationsByEventIdAsync(currentEvent.Id);
           
            ViewData["Message"] = "Media Delegation";
            
            return View(mediaDelegations);
        }

        [HttpGet]
        [TypeFilter(typeof(EventToViewAttribute))]
        [SessionAuthorize]
        public async Task<IActionResult> Edit(int? id)
        {
            int eventId = ViewBag.EventId;
            string userId = ViewBag.UserId;
            if (id == null)
            {
                return NotFound();
            }
            MediaDelegation selectedMedia = await _context.MediaDelegations.GetAsync((int)id);
            if (selectedMedia == null)
                return RedirectToAction(nameof(Delegations));

            MediaDelegation mediaDelegation = _context.MediaDelegations.
                                                                       GetDelegationByEventId(eventId, (int)id);

          
            IEnumerable<Country> countries = await _context.Countries.GetAllAsync();


            MediaDelegationIndexModel pim = new MediaDelegationIndexModel
            {
                Countries = countries,
                MediaDelegation = mediaDelegation,
                MediaCategories = await _context.MediaCategories.GetWithRelatedDataAsync()
            };

            return View(pim);

        }

        [HttpPost]
        [ExportModelState]
        [SessionAuthorize]
        [TypeFilter(typeof(EventToViewAttribute))]
        public async Task<IActionResult> Edit(MediaDelegationAdminEditModel part)
        {

            MediaDelegation mediaDelegation = await _context.MediaDelegations.GetWithRelatedDataByIdAsync(part.Id);
            if (mediaDelegation == null)
            {
                return RedirectToAction(nameof(Delegations));
            }

            if (part.Photo_image_path == null)
            {
                part.Photo_image_path = new FormFile(null, 0, 0, mediaDelegation.PhotoPath, mediaDelegation.PhotoPath);
            }

            if (ModelState.IsValid)
            {
                MediaSubCategory mediaSubCategory = await _context.MediaSubCategories.GetAsync(part.MediaSubCategoryId);
                Country country = await _context.Countries.GetAsync(part.CountryId);
                if (mediaSubCategory == null || country == null)
                {
                    ModelState.AddModelError("err", "Media sub Category or Country is invalid");
                    return RedirectToAction(nameof(Delegations));
                }


                int eventId = ViewBag.EventId;
                string appUserId = ViewBag.UserId;
                string path = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", eventId.ToString(), "media");


                Guid gPhoto = Guid.NewGuid();
                bool willSendMessage = false;

                try
                {
                    if (part.Press_image_path != null)
                    {
                        string pressCard = part.Press_image_path.ContentType;
                        string cPressCard = pressCard.Substring(pressCard.IndexOf("/") + 1).Trim();
                        Guid gPress = Guid.NewGuid();
                        string fullPathPress = Path.Combine(path, gPress.ToString() + "." + cPressCard);

                        if (mediaDelegation.PressCardPath != null)
                        {
                            _photoService.TryDelete(path, Path.Combine(path, mediaDelegation.PressCardPath));
                        }
                        if (equalsContentType(cPressCard, "png", "jpg", "jpeg", "pdf") == false)
                        {
                            ModelState.AddModelError("err", "File format not supported!!!");
                            return RedirectToAction(nameof(Delegations));
                        }
                        mediaDelegation.PressCardPath = String.Format("{0}.{1}", gPress.ToString(), cPressCard);

                        bool isUploadedPress = await _photoService.TryUploadAsync(part.Press_image_path, path, fullPathPress);
                        if (isUploadedPress)
                            mediaDelegation.PressCardPath = String.Format("{0}.{1}", gPress.ToString(), cPressCard);
                    }
                    if (mediaDelegation.MediaStatusType != part.MediaStatusType)
                        willSendMessage = true;//status is different..we must send a message...

                    if (part.Photo_image_path.FileName == mediaDelegation.PhotoPath)
                    {
                      
                        mediaDelegation.CountryId = part.CountryId;
                        mediaDelegation.Email = part.Email;
                        mediaDelegation.FirstName = part.FirstName;
                        mediaDelegation.LastName = part.LastName;
                        mediaDelegation.MediaSubCategoryId = part.MediaSubCategoryId;
                        mediaDelegation.MobilePhone = part.MobilePhone;
                        mediaDelegation.Nationality = part.Nationality;
                        mediaDelegation.MediaStatusType = part.MediaStatusType;
                        mediaDelegation.Comment = part.Comment;
                        mediaDelegation.MediaTitle = part.MediaTitle;
                        _context.Update<MediaDelegation>(mediaDelegation);
                        if (willSendMessage)
                        {
                            string mailMessage = String.Empty;
                         if(part.MediaStatusType == MediaStatusType.Rejected)
                            {
                              mailMessage = System.IO.File.ReadAllText(Path.Combine(_hostingEnvironment.WebRootPath, "templates", "MediaFail.html")); 
                            }
                         else if(part.MediaStatusType == MediaStatusType.Accepted)
                            {
                                mailMessage = System.IO.File.ReadAllText(Path.Combine(_hostingEnvironment.WebRootPath, "templates", "MediaSuccess.html"));
                            }
                            else if (part.MediaStatusType == MediaStatusType.Investigating)
                            {
                                mailMessage = System.IO.File.ReadAllText(Path.Combine(_hostingEnvironment.WebRootPath, "templates", "MediaQueue.html"));
                            }
                            mailMessage = mailMessage.Replace("<%FirstName%>", part.FirstName);
                            mailMessage = mailMessage.Replace("<%LastName%>", part.LastName);
                            mailMessage = mailMessage.Replace("<%eventName%>", ViewBag.CurrentEvent.Name);
                            _emailSender.SendEmailAsync(part.Email, "Media Registration for " + String.Format("{0} {1}",part.FirstName,part.LastName), mailMessage, (int)ViewBag.EventId,true);
                          
                        }
                          
                        await _context.SaveAsync();
                    }
                    else
                    {
                        string photo = part.Photo_image_path.ContentType;
                        string cType = photo.Substring(photo.IndexOf("/") + 1).Trim();



                        if (equalsContentType(cType, "png", "jpg", "jpeg", "pdf") == false)
                        {
                            ModelState.AddModelError("err", "File format not supported!!!");
                            return RedirectToAction(nameof(Delegations));
                        }

                        string fullPathPhoto = Path.Combine(path, gPhoto.ToString() + "." + cType);

                        bool IsDeleted = _photoService.TryDelete(path, Path.Combine(path, mediaDelegation.PhotoPath));
                        bool isUploadedPhoto = await _photoService.TryUploadAsync(part.Photo_image_path, path, fullPathPhoto);

                        if (isUploadedPhoto && IsDeleted)
                        {
                            mediaDelegation.CountryId = part.CountryId;
                            mediaDelegation.Email = part.Email;
                            mediaDelegation.FirstName = part.FirstName;
                            mediaDelegation.LastName = mediaDelegation.LastName;
                            mediaDelegation.MediaSubCategoryId = part.MediaSubCategoryId;
                            mediaDelegation.MobilePhone = part.MobilePhone;
                            mediaDelegation.Nationality = part.Nationality;
                            mediaDelegation.MediaStatusType = part.MediaStatusType;
                            mediaDelegation.Comment = part.Comment;
                            mediaDelegation.MediaTitle = part.MediaTitle;
                            mediaDelegation.PhotoPath = String.Format("{0}.{1}", gPhoto.ToString(), cType);
                            _context.Update<MediaDelegation>(mediaDelegation);
                            await _context.SaveAsync();
                            if (willSendMessage)
                            {
                                string mailMessage = String.Empty;
                                if (part.MediaStatusType == MediaStatusType.Rejected)
                                {
                                    mailMessage = System.IO.File.ReadAllText(Path.Combine(_hostingEnvironment.WebRootPath, "templates", "`"));
                                }
                                else if (part.MediaStatusType == MediaStatusType.Accepted)
                                {
                                    mailMessage = System.IO.File.ReadAllText(Path.Combine(_hostingEnvironment.WebRootPath, "templates", "MediaSuccess.html"));
                                }
                                else if (part.MediaStatusType == MediaStatusType.Investigating)
                                {
                                    mailMessage = System.IO.File.ReadAllText(Path.Combine(_hostingEnvironment.WebRootPath, "templates", "MediaQueue.html"));
                                }

                                mailMessage = mailMessage.Replace("<%FirstName%>", part.FirstName);
                                mailMessage = mailMessage.Replace("<%LastName%>", part.LastName);
                                mailMessage = mailMessage.Replace("<%eventName%>", ViewBag.CurrentEvent.Name);
                                _emailSender.SendEmailAsync(part.Email, "Media Registration for " + String.Format("{0} {1}", part.FirstName, part.LastName), mailMessage, (int)ViewBag.EventId, true);

                            }
                          
                        }
                    }

                }
                catch (Exception exp)
                {
                    ModelState.AddModelError("err", exp.Message);
                }


            }

            return RedirectToAction(nameof(Delegations));
        }



        [HttpPost]
        [TypeFilter(typeof(EventToViewAttribute))]
        public async Task<IActionResult> GetAllPressCards([FromBody]Search searchdata)
        {
            int eventId = (int)ViewBag.EventId;

            IEnumerable<string> musicNames = await _context.MediaDelegations.GetPressNamesAsync(searchdata.SearchData, eventId);

            if (musicNames == null)
                return Json(0);
            List<string> files = musicNames.Select(x => Path.Combine(_hostingEnvironment.WebRootPath, "uploads", eventId.ToString(), "media", x)).ToList();

            string userId = ViewBag.UserId;
            var archiveName = String.Format("mediapress{0}.zip", userId.ToString());
            var archive = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", archiveName);
            var archiveFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
            if (System.IO.File.Exists(archive))
            {
                System.IO.File.Delete(archive);
            }

            var temp = Path.Combine(_hostingEnvironment.WebRootPath, "temp");


            // empty the temp folder
            Directory.EnumerateFiles(temp).ToList().ForEach(f => System.IO.File.Delete(f));

            try
            {
                // copy the selected files to the temp folder
                files.ForEach(f => System.IO.File.Copy(f, Path.Combine(temp, Path.GetFileName(f)), true));


                // create a new archive
                ZipFile.CreateFromDirectory(temp, archive);
            }
            catch (Exception exp)
            {
                ModelState.AddModelError("err", exp.Message);
            }

            //return File(archive, "application/zip", "archive.zip");
            return Json(archiveName);
        }

        [HttpPost]
        [TypeFilter(typeof(EventToViewAttribute))]
        public async Task<IActionResult> GetAllPhotos([FromBody]Search searchdata)
        {
            int eventId = (int)ViewBag.EventId;

            IEnumerable<string> musicNames = await _context.MediaDelegations.GetPhotoNamesAsync(searchdata.SearchData, eventId);

            List<string> files = musicNames.Select(x => Path.Combine(_hostingEnvironment.WebRootPath, "uploads", eventId.ToString(), "media", x)).ToList();

            string userId = ViewBag.UserId;
            var archiveName = String.Format("media{0}.zip", userId.ToString());
            var archive = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", archiveName);
            var archiveFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
            if (System.IO.File.Exists(archive))
            {
                System.IO.File.Delete(archive);
            }

            var temp = Path.Combine(_hostingEnvironment.WebRootPath, "temp");


            // empty the temp folder
            Directory.EnumerateFiles(temp).ToList().ForEach(f => System.IO.File.Delete(f));

            try
            {
                // copy the selected files to the temp folder
                files.ForEach(f => System.IO.File.Copy(f, Path.Combine(temp, Path.GetFileName(f)), true));


                // create a new archive
                ZipFile.CreateFromDirectory(temp, archive);
            }
            catch (Exception exp)
            {
                ModelState.AddModelError("err", exp.Message);
            }

            //return File(archive, "application/zip", "archive.zip");
            return Json(archiveName);
        }
    }
}
