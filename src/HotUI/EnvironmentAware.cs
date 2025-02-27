using System;
using System.Collections.Generic;

namespace HotUI
{
    public abstract class ContextualObject
    {
        internal static readonly EnvironmentData Environment = new EnvironmentData();
        internal EnvironmentData _context;
        internal EnvironmentData Context(bool shouldCreate) => _context ?? (shouldCreate ? (_context = new EnvironmentData(this)) : null);

        internal EnvironmentData _localContext;
        internal EnvironmentData LocalContext(bool shouldCreate) => _localContext ?? (shouldCreate ? (_localContext = new EnvironmentData(this)) : null);


        protected ContextualObject()
        {

        }

        internal abstract void ContextPropertyChanged(string property, object value);

        public static string GetTypedKey(ContextualObject obj, string key)
            => GetTypedKey(obj.GetType(), key);
        public static string GetTypedKey(Type type, string key)
            => type == null ? key : $"{type.Name}.{key}";


        internal object GetValue(string key, ContextualObject current, View view,string typedKey)
        {
            try
            {
                //Check the local context
                var value = current == this ? LocalContext(false)?.GetValueInternal(key) : null; ;
                if (value != null)
                    return value;
                //Check the cascading context
                if (value == null)
                    value = Context(false)?.GetValueInternal(typedKey) ?? Context(false)?.GetValueInternal(key);
                //Check the parent
                if (value == null)
                {
                    //If no more parents, check the environment
                    if (view == null)
                        return View.Environment.GetValueInternal(typedKey) ?? View.Environment.GetValueInternal(key);
                    value = view.GetValue(key,current,view.Parent, typedKey);
                }
                return value;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        internal T GetValue<T>(string key, ContextualObject current, View view, string typedKey)
        {
            try
            {
                var value = GetValue(key, current, view,typedKey);
                return (T)value;
            }
            catch (Exception ex)
            {
                return default;
            }
        }

        internal void SetValue(string key, object value, bool cascades)
        {
            if (cascades)
                Context(true)?.SetValue(key, value);
            else
                LocalContext(true).SetValue(key, value);
        }
        
        
        //protected ICollection<string> GetAllKeys()
        //{
        //    //This is the global Environment
        //    if (View?.Parent == null)
        //        return dictionary.Keys;

        //    //TODO: we need a fancy way of collapsing this. This may be too slow
        //    var keys = new HashSet<string>();
        //    var localKeys = dictionary?.Keys;
        //    if (localKeys != null)
        //        foreach (var k in localKeys)
        //            keys.Add(k);

        //    var parentKeys = View?.Parent?.Context?.GetAllKeys() ?? View.Environment.GetAllKeys();
        //    if (parentKeys != null)
        //        foreach (var k in parentKeys)
        //            keys.Add(k);
        //    return keys;
        //}
    }

    public static class ContextualObjectExtensions
    {
        public static T SetEnvironment<T>(this T contextualObject, Type type, string key, object value, bool cascades = true)
            where T : ContextualObject
        {
            var typedKey = ContextualObject.GetTypedKey(type, key);
            contextualObject.SetValue(typedKey, value, cascades);
            //TODO: Verify this is needed 
            Device.InvokeOnMainThread(() => {
                contextualObject.ContextPropertyChanged(typedKey, value);
            });
            return contextualObject;
        }

        public static T SetEnvironment<T>(this T contextualObject, string key, object value, bool cascades = false)
            where T : ContextualObject
        {
            contextualObject.SetValue(key, value, cascades);
            Device.InvokeOnMainThread(() =>
            {
                contextualObject.ContextPropertyChanged(key, value);
            });
            return contextualObject;
        }

        //public static T SetEnvironment<T>(this T contextualObject, IDictionary<string, object> data, bool cascades = true) where T : ContextualObject
        //{
        //    foreach (var pair in data)
        //        contextualObject.SetValue(pair.Key, pair.Value,cascades);

        //    return contextualObject;
        //}

        public static T GetEnvironment<T>(this ContextualObject contextualObject, View view, string key) => contextualObject.GetEnvironment<T>(view, contextualObject.GetType(),key);
        public static T GetEnvironment<T>(this ContextualObject contextualObject, View view, Type type, string key) => contextualObject.GetValue<T>(key, contextualObject, view, ContextualObject.GetTypedKey(type ?? contextualObject.GetType(),key));
        public static object GetEnvironment(this ContextualObject contextualObject, View view, string key) => contextualObject.GetValue(key, contextualObject, view, ContextualObject.GetTypedKey(contextualObject, key));
        public static object GetEnvironment(this ContextualObject contextualObject, View view,Type type, string key) => contextualObject.GetValue(key, contextualObject, view, ContextualObject.GetTypedKey(type ?? contextualObject.GetType(), key));

        public static T GetEnvironment<T>(this View view, string key) => view.GetEnvironment<T>(view, view.GetType(), key);
        public static T GetEnvironment<T>(this View view, Type type, string key) => view.GetValue<T>(key, view, view.Parent,ContextualObject.GetTypedKey(type ?? view.GetType(), key));

        public static object GetEnvironment(this View view, string key) => view.GetValue(key, view, view.Parent, ContextualObject.GetTypedKey(view, key));
        public static object GetEnvironment(this View view, Type type, string key) => view.GetValue(key, view, view.Parent, ContextualObject.GetTypedKey(type ?? view.GetType(), key));
    }
}
