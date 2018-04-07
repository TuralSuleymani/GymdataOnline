using AccreditationMS.Areas.Admin.Models;
using AccreditationMS.Areas.Admin.Models.Meals;
using AccreditationMS.Core.UnitOfWork;
using AccreditationMS.Infrastructure;
using AccreditationMS.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccreditationMS.Infrastructure.Extensions;
using AccreditationMS.Areas.Admin.Models.Places;
using AccreditationMS.Models.Views.Meals;

namespace AccreditationMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("[controller]/[action]/{id?}")]
    [SessionAuthorize]

    public class MealsController : Controller
    {
        private readonly IUnitOfWork _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public MealsController(IUnitOfWork context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            this._userManager = userManager;
            this._roleManager = roleManager;
        }
         
        /// <summary>
        /// Add's errors to errorList model
        /// </summary>
        /// <param name="result"></param>
        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }


        [HttpGet]
        [Authorize]
        [SessionAuthorize]
        [ImportModelState]
        [TypeFilter(typeof(EventToViewAttribute))]
        public async Task<IActionResult> ManageMeals()
        {
            int? eventId = (int?)ViewBag.EventId;
             
            if (eventId == null)
                return NotFound();

          
           
            string appUserId = ViewBag.UserId;

            Event currentEvent = ViewBag.CurrentEvent;
            ViewBag.CurrentCurrency = (await _context.Currencies.GetWithRelatedDataByIdAsync(currentEvent.CurrencyId)).Unit;

            ViewBag.DeadLine = currentEvent.MealsEndDate.ToString("dd-MM-yyyy");


            //if user id is empty then try get user id from session
            if (String.IsNullOrEmpty(appUserId))
                appUserId = (await _userManager.GetUserAsync(HttpContext.User)).Id;

            //if session not contains user,then redirect..
            if (appUserId == null)
                return NotFound();
            IEnumerable<MealReservationAdminIndexModel> result = await GetMeals(currentEvent);
            return View(result);
        }



        /// <summary>
        /// Edit selected Meal information
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [TypeFilter(typeof(EventToViewAttribute))]
        public async Task<IActionResult> ManageEdit(int? id)
        {
            int eventId = ViewBag.EventId;
            if (id == null)
            {
                return NotFound();
            }

            MealsReservation mealsReservation = (await _context.MealsReservation.GetWithRelatedDataByIdAsync((int)id));

            if (mealsReservation != null)
            {

                MealsReservationEditModel mealsReservationEditModel = new MealsReservationEditModel
                {
                    MealsReservation = mealsReservation,
                    Venues = (await _context.Meals.GetWithRelatedDataByEventIdAsync(eventId))
                                                          .Select(x => new VenueModel { VenueId = x.Id, VenueName = x.VenueName })

                };

                return View(mealsReservationEditModel);

            }
            else
                return RedirectToAction(nameof(ManageMeals));
        }


        /// <summary>
        /// Update Meal information from Form
        /// </summary>
        /// <param name="placeReservation"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ExportModelState]
        [TypeFilter(typeof(EventToViewAttribute))]
        public async Task<IActionResult> ManageEdit(int? id, int MealTypeId, [Bind("Id,MealId,CheckInDate,MealType,PricePerPerson,Quantity,SubTotal")]MealsReservation mealsReservation)
        {
            if (id == null || mealsReservation?.Id != id || (MealTypeId < 0 || MealTypeId > 3))
                return NotFound();

            int eventId = ViewBag.EventId;
            mealsReservation.MealType = (MealType)MealTypeId;

            MealsReservation currentMealsReservation = await _context.MealsReservation.GetAsync((int)id);

            if (ModelState.IsValid && currentMealsReservation != null)
            {

                var loggableMeals = currentMealsReservation.Clone();
                Meal currentMeal = (await _context.Meals.GetAllByEventIdAsync((int)eventId)).Where(x => x.Id == mealsReservation.MealId).SingleOrDefault();

                float? price = mealsReservation.MealType == MealType.Breakfast
                                          ? currentMeal.BreakfastPrice : mealsReservation.MealType == MealType.Lunch
                                           ? currentMeal.LunchPrice : currentMeal.DinnerPrice;
                if (price == null)
                {
                    ModelState.AddModelError("err", "Price is not correct!!");
                }
                else
                {
                    currentMealsReservation.PricePerPerson = (decimal)(float)price;
                    currentMealsReservation.SubTotal = mealsReservation.Quantity * Convert.ToDecimal(price);
                    currentMealsReservation.Quantity = mealsReservation.Quantity;
                    currentMealsReservation.UpdatedAt = DateTime.Now;
                  //  currentMealsReservation.AppUserId = ViewBag.UserId;
                    currentMealsReservation.MealId = mealsReservation.MealId;
                    currentMealsReservation.MealType = mealsReservation.MealType;
                    _context.Update<MealsReservation>(currentMealsReservation);

                    //try to log
                    await this.LogForUpdateAsync<MealsReservation>(_context.LiveLogs, loggableMeals, currentMealsReservation, "Accommodation-Meals(Administration)");
                    //commit
                    await _context.SaveAsync();
                }

            }
            else
            {
                return View(mealsReservation);
            }
            return RedirectToAction(nameof(ManageMeals));
        }




        /// <summary>
        /// Get Meals data async using ajax
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [TypeFilter(typeof(EventToViewAttribute))]
        public async Task<IActionResult> GetMeals(DataTableAjaxPostModel data)
        {
            int eventId = (int)ViewBag.EventId;
             
            int itemsCount = await _context.MealsReservation.CountByEventIdAsync(eventId);

            Column orderableColumn = data.columns[data.order[0].column];
            string column = orderableColumn.data.ToCapitalizedJson();

            IEnumerable<MealReservationAdminIndexModel> mealReservations = await _context.MealsReservation.GetOrderedPagingByEventIdFunction(eventId, data.start, data.length, orderableColumn.data, data.order[0].dir, data.search?.value);

            JsonResult jsonResult = Json(new
            {
                draw = data?.draw ?? 0,
                recordsTotal = itemsCount,
                recordsFiltered = itemsCount,
                data = mealReservations ?? new List<MealReservationAdminIndexModel>()
            });
            return jsonResult;
        }

        [HttpPost]
        [TypeFilter(typeof(EventToViewAttribute))]
        public async  Task<IEnumerable<MealReservationAdminIndexModel>> GetMeals(Event currentEvent)
        {
             IEnumerable<MealReservationAdminIndexModel> mealReservations = await _context.MealsReservation.GetOrderedPagingByEventIdAsync(currentEvent.Id);

            return mealReservations;
        }

        /// <summary>
        /// View Meals information
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ImportModelState]
        [SessionAuthorize]
        [TypeFilter(typeof(EventToViewAttribute))]
        public async Task<IActionResult> Index()
        {
            //get current user
            AppUser appUser = await _userManager.GetUserAsync(HttpContext.User);

            //get the biggest role for this user
            RoleDependency biggestRoleforCurrentUser = await _roleManager.GetBiggestRoleAsync(_userManager, appUser, _context.RoleDependencies);

            //assign it to dynamic View for comparing
            ViewBag.BiggestRoleForCurrentUser = biggestRoleforCurrentUser;
            IEnumerable<MealsIndexViewModel> mealsData = new List<MealsIndexViewModel>();
            if ((await _context.Meals.CountAsyncByEvent((int)ViewBag.EventId)) != 0)
            {
             mealsData =   (await _context.Meals
                                .GetAllByEventIdAsync((int)ViewBag.EventId))
                                   .Select(x =>
                                   new MealsIndexViewModel
                                   {
                                       Id = x.Id
                                        ,
                                       BreakfastPrice = x.BreakfastPrice
                                        ,
                                       DinnerPrice = x.DinnerPrice
                                        ,
                                       EventId = x.EventId
                                        ,
                                       EventName = ((Event)ViewBag.CurrentEvent).Name
                                        ,
                                       LunchPrice = x.LunchPrice
                                        ,
                                       VenueName = x.VenueName,
                                       CreatedBy = x.AppUserId
                                   });
            }

           
            MealsEventModel mealsEventModel = new MealsEventModel
            {
                MealsIndexViewModel = mealsData
                , EventList = new SimpleEventModel { EventId = (int)ViewBag.EventId, EventName = (string)ViewBag.EventName }
            };

           return View(mealsEventModel);
        }

        /// <summary>
        /// Create new Meal
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost]
        [ExportModelState]
        [SessionAuthorize]
        [TypeFilter(typeof(EventToViewAttribute))]
        public async Task<IActionResult> Create(MealsIndexViewModel model)
        {
            
            if(ModelState.IsValid)
            {
                try
                {
                    Meal meal = new Meal
                    {
                        EventId = model.EventId,
                        BreakfastPrice = model.BreakfastPrice,
                        DinnerPrice = model.DinnerPrice,
                        LunchPrice = model.LunchPrice,
                        VenueName = model.VenueName,
                        AppUserId = (string)ViewBag.UserId

                    };
                    await _context.Meals.AddAsync(meal);
                   await _context.SaveAsync();
                }
                catch(Exception exp)
                {
                    ModelState.AddModelError("err", exp.Message);
                }
               
            }
          
            return RedirectToAction(nameof(Index));
        }



        /// <summary>
        /// Edit Meal detail by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [SessionAuthorize]
        [ExportModelState]
        [TypeFilter(typeof(EventToViewAttribute))]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            Meal meal =await _context.Meals.GetAsync((int)id); 
            if (meal != null)
            {
                //send selected meal and active event list to edit view
                MealsEventModel mealsEventModel = new MealsEventModel
                {
                    EventList = new SimpleEventModel { EventId = (int)ViewBag.EventId, EventName = (string)ViewBag.EventName }
                                                          ,CurrentMeal = new MealsIndexViewModel
                                                          {
                                                              Id=meal.Id
                                                              ,BreakfastPrice=meal.BreakfastPrice
                                                              ,DinnerPrice=meal.DinnerPrice
                                                              ,EventId=meal.EventId
                                                              ,EventName=(string)ViewBag.EventName
                                                              , LunchPrice=meal.LunchPrice
                                                              ,VenueName=meal.VenueName
                                                          }
                };
                return View(mealsEventModel);
            }
            else
              return RedirectToAction(nameof(Index));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Edit selected meal information by Id via POST </summary>
        ///
        /// <remarks>   User, 09.10.2017. </remarks>
        ///
        /// <param name="id">       . </param>
        /// <param name="model">    The model. </param>
        ///
        /// <returns>   An asynchronous result that yields an IActionResult. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPost]
        [SessionAuthorize]
        [ExportModelState]
        [TypeFilter(typeof(EventToViewAttribute))]
        public async Task<IActionResult> Edit(int? id,MealsIndexViewModel model)
        {
            if (id == null || id != model.Id)
                return NotFound();

          Event selectedEvent = _context.Events.FindOnly(x => x.Id == model.EventId).SingleOrDefault();

            if (selectedEvent == null)
                return NotFound();

            if (ModelState.IsValid)
            {
               Meal currentMeal = _context.Meals.Find(x => x.Id == model.Id).SingleOrDefault(); 

                if(currentMeal!=null)
                {
                    using (var transaction = _context.Database.BeginTransaction())
                    {
                      try
                        {
                            //delete first
                            _context.Meals.Remove(currentMeal);
                           await _context.SaveAsync();
                            currentMeal = new Meal();
                            //then add new one
                            currentMeal.VenueName = model.VenueName;
                            currentMeal.BreakfastPrice = model.BreakfastPrice;
                            currentMeal.DinnerPrice = model.DinnerPrice;
                            currentMeal.LunchPrice = model.LunchPrice;
                            currentMeal.EventId = model.EventId;
                            currentMeal.AppUserId = ViewBag.UserId;

                           await _context.Meals.AddAsync(currentMeal);
                            await _context.SaveAsync();

                            //commit transaction
                            transaction.Commit();
                        }
                        catch(Exception exp)
                        {
                            ModelState.AddModelError("err", exp.Message);
                            transaction.Rollback();
                        }
                    }
                  
                   
                }
                return RedirectToAction(nameof(Index));
            }
            else
                return View(model);
        }


        [HttpPost]
        [TypeFilter(typeof(EventToViewAttribute))]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plc = await _context.Meals.GetAsync((int)id);

            if (plc == null)
            {
                return NotFound();
            }
            //plc.AppUserId = ViewBag.UserId;
            await this.LogForDeleteAsync<Meal>(_context.LiveLogs, plc, "Accommodation-Meals(Administration)");

            _context.Meals.Remove(plc);

            await _context.SaveAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [TypeFilter(typeof(EventToViewAttribute))]
        public async Task<IActionResult> ManageDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plc = await _context.MealsReservation.GetAsync((int)id);

            if (plc == null)
            {
                return NotFound();
            }
            //plc.AppUserId = ViewBag.UserId;
            await this.LogForDeleteAsync<MealsReservation>(_context.LiveLogs, plc, "Accommodation-Meals(Administration)");

            _context.MealsReservation.Remove(plc);

            await _context.SaveAsync();

            return RedirectToAction(nameof(ManageMeals));
        }

    }
}
