﻿using System;
using System.Linq;
using VVVV.Hosting.Pins.Config;
using VVVV.Hosting.Pins.Input;
using VVVV.Hosting.Pins.Output;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.IO
{
    class SpreadRegistry : IORegistryBase
    {
        public SpreadRegistry()
        {
            RegisterInput(typeof(ISpread<>), (factory, context) => {
                              var attribute = context.IOAttribute;
                              var t = context.DataType;
                              ISpread spread = null;
                              if (t.IsGenericType && t.GetGenericArguments().Length == 1)
                              {
                                  if (typeof(ISpread<>).MakeGenericType(t.GetGenericArguments().First()).IsAssignableFrom(t))
                                  {
                                      var spreadType = typeof(InputBinSpread<>).MakeGenericType(t.GetGenericArguments().First());
                                      
                                      if (attribute.IsPinGroup)
                                      {
                                          spreadType = typeof(InputSpreadList<>).MakeGenericType(t.GetGenericArguments().First());
                                      }
                                      
                                      spread = Activator.CreateInstance(spreadType, factory, attribute.Clone()) as ISpread;
                                      if (attribute.AutoValidate)
                                          return new GenericIOContainer<ISpread>(context, factory, spread, s => s.Sync());
                                      else
                                          return new GenericIOContainer<ISpread>(context, factory, spread);
                                  }
                              }
                              var container = factory.CreateIOContainer(typeof(IInStream<>).MakeGenericType(context.DataType), attribute, false);
                              var pinType = typeof(InputPin<>).MakeGenericType(context.DataType);
                              spread = Activator.CreateInstance(pinType, factory, container.GetPluginIO(), container.RawIOObject) as ISpread;
                              if (attribute.AutoValidate)
                                  return IOContainer.Create(context, spread, container, p => p.Sync());
                              else
                                  return IOContainer.Create(context, spread, container);
                          });
            
            RegisterInput(typeof(IDiffSpread<>), (factory, context) => {
                              var attribute = context.IOAttribute;
                              var t = context.DataType;
                              attribute.CheckIfChanged = true;
                              ISpread spread = null;
                              
                              if (t.IsGenericType && t.GetGenericArguments().Length == 1)
                              {
                                  if (typeof(ISpread<>).MakeGenericType(t.GetGenericArguments().First()).IsAssignableFrom(t))
                                  {
                                      var spreadType = typeof(DiffInputBinSpread<>).MakeGenericType(t.GetGenericArguments().First());
                                      
                                      if (attribute.IsPinGroup)
                                      {
                                          spreadType = typeof(DiffInputSpreadList<>).MakeGenericType(t.GetGenericArguments().First());
                                      }
                                      
                                      spread = Activator.CreateInstance(spreadType, factory, attribute.Clone()) as ISpread;
                                      if (attribute.AutoValidate)
                                          return new GenericIOContainer<ISpread>(context, factory, spread, s => s.Sync(), s => s.Flush());
                                      else
                                          return new GenericIOContainer<ISpread>(context, factory, spread, null, s => s.Flush());
                                  }
                              }
                              var container = factory.CreateIOContainer(typeof(IInStream<>).MakeGenericType(context.DataType), attribute, false);
                              var pinType = typeof(DiffInputPin<>).MakeGenericType(context.DataType);
                              spread = Activator.CreateInstance(pinType, factory, container.GetPluginIO(), container.RawIOObject) as ISpread;
                              if (attribute.AutoValidate)
                                  return IOContainer.Create(context, spread, container, s => s.Sync());
                              else
                                  return IOContainer.Create(context, spread, container);
                          },
                          false);
            
            RegisterOutput(typeof(ISpread<>), (factory, context) => {
                               var attribute = context.IOAttribute;
                               var t = context.DataType;
                               if (t.IsGenericType && t.GetGenericArguments().Length == 1)
                               {
                                   if (typeof(ISpread<>).MakeGenericType(t.GetGenericArguments().First()).IsAssignableFrom(t))
                                   {
                                       var spreadType = typeof(OutputBinSpread<>).MakeGenericType(t.GetGenericArguments().First());
                                       
                                       if (attribute.IsPinGroup)
                                       {
                                           spreadType = typeof(OutputSpreadList<>).MakeGenericType(t.GetGenericArguments().First());
                                       }
                                       
                                       var spread = Activator.CreateInstance(spreadType, factory, attribute.Clone()) as ISpread;
                                       return new GenericIOContainer<ISpread>(context, factory, spread, null, s => s.Flush());
                                   }
                               }
                               var container = factory.CreateIOContainer(typeof(IOutStream<>).MakeGenericType(context.DataType), attribute, false);
                               var pinType = typeof(OutputPin<>).MakeGenericType(context.DataType);
                               var pin = Activator.CreateInstance(pinType, factory, container.GetPluginIO(), container.RawIOObject) as ISpread;
                               return IOContainer.Create(context, pin, container, null, p => p.Flush());
                           });
            
            RegisterConfig(typeof(IDiffSpread<>),
                           (factory, context) =>
                           {
                               var attribute = context.IOAttribute;
                               var container = factory.CreateIOContainer(typeof(IIOStream<>).MakeGenericType(context.DataType), attribute, false);
                               var pinType = typeof(ConfigPin<>).MakeGenericType(context.DataType);
                               var spread = (IDiffSpread) Activator.CreateInstance(pinType, factory, container.GetPluginIO(), container.RawIOObject);
                               return IOContainer.Create(context, spread, container, null, s => s.Flush(), p => p.Sync());
                           },
                           true);
        }
    }
}
