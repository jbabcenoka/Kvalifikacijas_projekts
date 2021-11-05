//using system;
//using system.collections.generic;
//using system.linq;
//using system.threading.tasks;
//using microsoft.aspnetcore.mvc;
//using microsoft.aspnetcore.mvc.rendering;
//using microsoft.entityframeworkcore;
//using programmingcoursesapp.data;
//using programmingcoursesapp.models;

//namespace programmingcoursesapp
//{
//    public class exercisescontroller : controller
//    {
//        private readonly applicationdbcontext _context;

//        public exercisescontroller(applicationdbcontext context)
//        {
//            _context = context;
//        }

//        // get: exercises
//        public async task<iactionresult> index()
//        {
//            return view(await _context.exercises.tolistasync());
//        }

//        // get: exercises/details/5
//        public async task<iactionresult> details(int? id)
//        {
//            if (id == null)
//            {
//                return notfound();
//            }

//            var exercise = await _context.exercises
//                .firstordefaultasync(m => m.id == id);
//            if (exercise == null)
//            {
//                return notfound();
//            }

//            return view(exercise);
//        }

//        // get: exercises/create
//        public iactionresult create()
//        {
//            return view();
//        }

//        // post: exercises/create
//        // to protect from overposting attacks, enable the specific properties you want to bind to.
//        // for more details, see http://go.microsoft.com/fwlink/?linkid=317598.
//        [httppost]
//        [validateantiforgerytoken]
//        public async task<iactionresult> create([bind("questiontext,id,name")] exercise exercise)
//        {
//            if (modelstate.isvalid)
//            {
//                _context.add(exercise);
//                await _context.savechangesasync();
//                return redirecttoaction(nameof(index));
//            }
//            return view(exercise);
//        }

//        // get: exercises/edit/5
//        public async task<iactionresult> edit(int? id)
//        {
//            if (id == null)
//            {
//                return notfound();
//            }

//            var exercise = await _context.exercises.findasync(id);
//            if (exercise == null)
//            {
//                return notfound();
//            }
//            return view(exercise);
//        }

//        // post: exercises/edit/5
//        // to protect from overposting attacks, enable the specific properties you want to bind to.
//        // for more details, see http://go.microsoft.com/fwlink/?linkid=317598.
//        [httppost]
//        [validateantiforgerytoken]
//        public async task<iactionresult> edit(int id, [bind("questiontext,id,name")] exercise exercise)
//        {
//            if (id != exercise.id)
//            {
//                return notfound();
//            }

//            if (modelstate.isvalid)
//            {
//                try
//                {
//                    _context.update(exercise);
//                    await _context.savechangesasync();
//                }
//                catch (dbupdateconcurrencyexception)
//                {
//                    if (!exerciseexists(exercise.id))
//                    {
//                        return notfound();
//                    }
//                    else
//                    {
//                        throw;
//                    }
//                }
//                return redirecttoaction(nameof(index));
//            }
//            return view(exercise);
//        }

//        // get: exercises/delete/5
//        public async task<iactionresult> delete(int? id)
//        {
//            if (id == null)
//            {
//                return notfound();
//            }

//            var exercise = await _context.exercises
//                .firstordefaultasync(m => m.id == id);
//            if (exercise == null)
//            {
//                return notfound();
//            }

//            return view(exercise);
//        }

//        // post: exercises/delete/5
//        [httppost, actionname("delete")]
//        [validateantiforgerytoken]
//        public async task<iactionresult> deleteconfirmed(int id)
//        {
//            var exercise = await _context.exercises.findasync(id);
//            _context.exercises.remove(exercise);
//            await _context.savechangesasync();
//            return redirecttoaction(nameof(index));
//        }

//        private bool exerciseexists(int id)
//        {
//            return _context.exercises.any(e => e.id == id);
//        }
//    }
//}
