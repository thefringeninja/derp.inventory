using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nancy;
using Nancy.Extensions;
using Nancy.ModelBinding;

namespace Derp.Inventory.Web.Modules
{
    public class CommandBinder<TCommand> : IModelBinder where TCommand : class
    {
        private readonly Func<TCommand> createInstance;
        private readonly BindingDefaults defaults;
        private readonly IFieldNameConverter fieldNameConverter;
        private readonly IEnumerable<ITypeConverter> typeConverters;
        private readonly bool typeParameterHasNoPublicSettableProperties;
        private IEnumerable<IBodyDeserializer> bodyDeserializers;

        public CommandBinder(IEnumerable<ITypeConverter> typeConverters,
                             IEnumerable<IBodyDeserializer> bodyDeserializers,
                             IFieldNameConverter fieldNameConverter, BindingDefaults defaults)
        {
            typeParameterHasNoPublicSettableProperties = false ==
                                                         typeof (TCommand).GetProperties().Any(pi => pi.CanWrite);

            if (false == typeParameterHasNoPublicSettableProperties) return; // no point in continuing here

            if (typeConverters == null)
            {
                throw new ArgumentNullException("typeConverters");
            }

            if (bodyDeserializers == null)
            {
                throw new ArgumentNullException("bodyDeserializers");
            }

            if (fieldNameConverter == null)
            {
                throw new ArgumentNullException("fieldNameConverter");
            }

            if (defaults == null)
            {
                throw new ArgumentNullException("defaults");
            }

            this.typeConverters = typeConverters;
            this.bodyDeserializers = bodyDeserializers;
            this.fieldNameConverter = fieldNameConverter;
            this.defaults = defaults;

            createInstance = CreateFactory();
        }

        #region IModelBinder Members

        public object Bind(NancyContext context, Type modelType, object suppliedInstance,
                           BindingConfig configuration,
                           params string[] blackList)
        {
            var instance = suppliedInstance as TCommand ?? createInstance();

            var bindingContext = CreateBindingContext(context, modelType, instance, configuration,
                                                      blackList);

            var validFields = GetFields(blackList);
            var bindingExceptions = new List<PropertyBindingException>();
            foreach (var modelField in validFields)
            {
                var existingValue =
                    modelField.GetValue(bindingContext.Model);

                var stringValue = GetValue(modelField.Name, bindingContext);

                if (String.IsNullOrEmpty(stringValue) ||
                    (!IsDefaultValue(existingValue, modelField.FieldType) && !bindingContext.Configuration.Overwrite))
                    continue;
                try
                {
                    BindField(modelField, stringValue, bindingContext);
                }
                catch (PropertyBindingException ex)
                {
                    bindingExceptions.Add(ex);
                }
            }

            if (bindingExceptions.Any())
            {
                throw new ModelBindingException(modelType, bindingExceptions);
            }

            return bindingContext.Model;
        }

        public bool CanBind(Type modelType)
        {
            return typeParameterHasNoPublicSettableProperties && modelType == typeof (TCommand);
        }

        #endregion

        private static Func<TCommand> CreateFactory()
        {
            const BindingFlags instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var defaultContructors =
                from ctor in typeof (TCommand).GetConstructors(instance)
                where false == ctor.GetParameters().Any()
                select ctor;

            var defaultConstructor = defaultContructors.FirstOrDefault();
            if (defaultConstructor == null)
                throw new ArgumentException(String.Format(
                    "The type {0} does not appear to have a default constructor.", typeof (TCommand)));

            return () => (TCommand) defaultConstructor.Invoke(new object[0]);
        }

        private static void SetFieldValue(FieldInfo modelField, object model, object value)
        {
            // TODO - catch reflection exceptions?
            modelField.SetValue(model, value);
        }

        private static bool IsDefaultValue(object existingValue, Type fieldType)
        {
            return fieldType.IsValueType
                       ? Equals(existingValue, Activator.CreateInstance(fieldType))
                       : existingValue == null;
        }


        private static void BindField(FieldInfo modelField, string stringValue, BindingContext context)
        {
            var destinationType = modelField.FieldType;

            var typeConverter =
                context.TypeConverters.FirstOrDefault(c => c.CanConvertTo(destinationType, context));

            if (typeConverter != null)
            {
                try
                {
                    SetFieldValue(modelField, context.Model,
                                  typeConverter.Convert(stringValue, destinationType, context));
                }
                catch (Exception e)
                {
                    throw new PropertyBindingException(modelField.Name, stringValue, e);
                }
            }
            else if (destinationType == typeof (string))
            {
                SetFieldValue(modelField, context.Model, stringValue);
            }
        }


        private BindingContext CreateBindingContext(NancyContext context, Type modelType, object instance,
                                                    BindingConfig configuration, IEnumerable<string> blackList)
        {
            return new BindingContext
            {
                Configuration = configuration,
                Context = context,
                Model = instance,
                DestinationType = typeof (TCommand),
                RequestData = GetDataFields(context),
                TypeConverters = typeConverters.Concat(defaults.DefaultTypeConverters),
            };
        }

        private static IEnumerable<FieldInfo> GetFields(IEnumerable<string> blackList)
        {
            return from fieldInfo in typeof (TCommand).GetFields()
                   where false == blackList.Contains(fieldInfo.Name, StringComparer.InvariantCulture)
                   select fieldInfo;
        }


        private IDictionary<string, string> GetDataFields(NancyContext context)
        {
            var dictionaries = new IDictionary<string, string>[]
            {
                ConvertDynamicDictionary(context.Request.Form),
                ConvertDynamicDictionary(context.Request.Query),
                ConvertDynamicDictionary(context.Parameters)
            };

            return dictionaries.Merge();
        }

        private IDictionary<string, string> ConvertDynamicDictionary(DynamicDictionary dictionary)
        {
            if (dictionary == null)
            {
                return null;
            }

            return dictionary.GetDynamicMemberNames().ToDictionary(
                memberName => fieldNameConverter.Convert(memberName).Underscore().Pascalize(),
                memberName => (string) dictionary[memberName]);
        }

        private static string GetValue(string fieldName, BindingContext context)
        {
            return context.RequestData.ContainsKey(fieldName) ? context.RequestData[fieldName] : String.Empty;
        }
    }
}