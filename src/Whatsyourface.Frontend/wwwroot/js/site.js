// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.

var wyfCommon = {
    createCultureCookie: function (cookieName, cultureString) {
        return `${cookieName}=c=${cultureString}|uic=${cultureString}`;
    },

    setCultureCookie: function (cookieName, cultureString) {
        document.cookie = wyfCommon.createCultureCookie(cookieName, cultureString);
    }
}