using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace QuickMenu
{
    public interface IMenuItem
    {
        public bool visible { get; }
        public int priority { get; }
        public string title { get; }
        public string description { get; }
        public string category { get; }
        public string subCategory { get; }

        public bool Validation(Context context);

        public bool Command(Context context);

        public IEnumerable<VisualElement> GetParameterFields();

        public string GetMatchcode()
        {
            return $"{title} {category} {subCategory} {description}";
        }

        public Color GetCategoryColor(float alpha)
        {
            var category = Utils.GetCategory(this.category);

            var color = category.color;
            color.r *= alpha;
            color.g *= alpha;
            color.b *= alpha;
            return color;
        }

        public Color GetSubCategoryColor(float alpha)
        {
            var category = Utils.GetCategory(this.category);

            var subCategory = Utils.GetCategory(this.subCategory);
            subCategory.color.a = 1;

            var color = Color.Lerp(category.color, subCategory.color, .5f);
            color.r *= alpha;
            color.g *= alpha;
            color.b *= alpha;
            return color;
        }
    }
}