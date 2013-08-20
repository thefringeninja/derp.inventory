using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Nancy.ViewEngines.Razor;

namespace Derp.Inventory.Web.views
{
    public class FormHelper<T> where T : class
    {
        private readonly HtmlHelpers<T> helpers;

        public FormHelper(HtmlHelpers<T> helpers)
        {
            this.helpers = helpers;
        }

        public string Action
        {
            get
            {
                var useCase = typeof (T);
                return DispatchPath(useCase);
            }
        }

        private string DispatchPath(Type useCase)
        {
            return helpers.RenderContext.ParsePath("~" + useCase.GetDispatcherResource());
        }

        private string GetName<TProperty>(Expression<Func<T, TProperty>> property)
        {
            var me = property.Body as MemberExpression;
            if (me == null) throw new ArgumentException();
            return me.Member.Name.Underscore().Dasherize();
        }

        public IHtmlString HiddenFor<TProperty>(Expression<Func<T, TProperty>> property)
        {
            return HiddenFor(property, property.Compile()(helpers.Model));
        }

        public IHtmlString HiddenFor<TProperty>(Expression<Func<T, TProperty>> property, object value)
        {
            return new HelperResult(writer =>
            {
                writer.Write("<input type='hidden' name='");
                writer.Write(GetName(property));
                writer.Write("'");
                if (value != null)
                {
                    writer.Write(" value='");
                    writer.Write(value);
                    writer.Write("'");
                }
                writer.Write(" />");
            });
        }

        public IHtmlString TextBoxFor<TProperty>(Expression<Func<T, TProperty>> property,
                                                 object attributes = null)
        {
            return TextBoxFor(property, property.Compile()(helpers.Model), attributes);
        }

        public IHtmlString TextBoxFor<TProperty>(Expression<Func<T, TProperty>> property, object value,
                                                 object attributes = null)
        {
            return new HelperResult(writer =>
            {
                writer.Write("<input type='");
                writer.Write(typeof (TProperty).GetInputType());
                writer.Write("' name='");
                writer.Write(GetName(property));
                if (value != null)
                {
                    writer.Write("' value='");
                    writer.Write(value);
                }
                writer.Write("' />");
                writer.WriteLine();
            });
        }

        public IHtmlString TextAreaFor(Expression<Func<T, string>> property,
                                       object attributes = null)
        {
            return TextAreaFor(property, property.Compile()(helpers.Model), attributes);
        }

        public IHtmlString TextAreaFor(Expression<Func<T, string>> property, string value,
                                       object attributes = null)
        {
            return new HelperResult(writer =>
            {
                writer.Write("<textarea name='");
                writer.Write(GetName(property));
                writer.Write("'>");
                writer.Write(value ?? String.Empty);
                writer.Write("</textarea>");
                writer.WriteLine();
            });
        }

        public IHtmlString LabelFor<TProperty>(Expression<Func<T, TProperty>> property, object attributes = null)
        {
            return new HelperResult(writer =>
            {
                writer.Write("<label>");
                writer.Write(GetName(property).Underscore().Titleize());
                writer.Write("</label>");
                writer.WriteLine();
            });
        }

        public IHtmlString Submit(string label = null, object attributes = null)
        {
            label = label ?? typeof (T).Name.Underscore().Titleize();
            return new HelperResult(writer =>
            {
                writer.Write("<input type='submit' value='");
                writer.Write(label);
                writer.Write("' />");
            });
        }

        public IHtmlString Link<TCommand>(
            string text = null,
            TCommand query = null,
            object attributes = null) where TCommand : class
        {
            return new HelperResult(writer =>
            {
                var builder = new StringBuilder()
                    .Append("<a href=\"")
                    .Append(DispatchPath(typeof (TCommand)));

                if (query != null)
                {
                    builder.Append("?").Append(
                        String.Join(
                            "&",
                            InputModel(query).Select(
                                input => input.Item1.Underscore().Dasherize() + "=" + input.Item3)));
                }

                builder.Append("\">")
                       .Append(text ?? typeof (TCommand).Name.Underscore().Humanize())
                       .Append("</a>");

                writer.WriteLine(builder.ToString());
            });
        }

        public IHtmlString Partial<TCommand>(TCommand command) where TCommand : class
        {
            return helpers.Partial(
                DispatchPath(typeof (TCommand)),
                command);
        }

        private static IEnumerable<Tuple<String, String, Object>> InputModel<TCommand>(TCommand command)
        {
            return from property in typeof (TCommand).GetFields()
                   select Tuple.Create(
                       property.Name,
                       property.FieldType.GetInputType(),
                       property.GetValue(command));
        }
    }
}