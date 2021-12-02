//     Unit UnsafeEnterAsync(int epoch)
//     {
//         var loop = true;
//         while (loop)
//         {
//             var oldState = Volatile.Read(ref state);
//             switch (oldState.Tag)
//             {
//                 case FiberStateTags.Executing:
//                     var executing = (Executing<A>)oldState;
//                     var newStatus = FiberStatus.Suspended(executing.Status, Interruptible.Value && !Interrupting.Value, epoch);
//                     var newState = FiberState.Executing<A>(newStatus, executing.Callbacks, CancelerState.Pending());
//                     loop = (state != oldState || Interlocked.CompareExchange(ref state, newState, oldState) != oldState);
//                     break;
//                 case FiberStateTags.Done:
//                     throw new Exception("Fiber being completed multiple times");
//             }
//         }
//         return Unit();
//     }

//     bool UnsafeExitAsync(int epoch)
//     {
//         var loop = true;
//         var result = false;
//         while (loop)
//         {
//             var oldState = Volatile.Read(ref state);
//             switch (oldState.Tag)
//             {
//                 case FiberStateTags.Executing:
//                     var executing = (Executing<A>)oldState;
//                     switch (executing.Status.Tag)
//                     {
//                         case FiberStatusTags.Suspended:
//                             var suspended = (Suspended)executing.Status;
//                             if (epoch != suspended.Epoch)
//                             {
//                                 result = false;
//                                 loop = false;
//                                 break;
//                             }
//                             var newState = FiberState.Executing<A>(executing.Status, executing.Callbacks, CancelerState.Empty());
//                             loop = (state != oldState || Interlocked.CompareExchange(ref state, newState, oldState) != oldState);
//                             break;
//                         default:
//                             result = false;
//                             loop = false;
//                             break;
//                     }
//                     break;
//                 case FiberStateTags.Done:
//                     result = false;
//                     loop = false;
//                     break;
//             }
//         }
//         return result;
//     }

//     Func<dynamic, Unit> UnsafeCreateAsyncResume(int epoch) => mio => 
//     {
//         if (UnsafeExitAsync(epoch)) 
//         {
//             return UnsafeRunLater(mio);
//         }
//         return Unit();
//     };