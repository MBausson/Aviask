var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
const weekCountElement = document.getElementById("week-count");
const lifetimeRatioElement = document.getElementById("lifetime-ratio");
const favoriteThemesListElement = document.getElementById("list-favourite-themes");
const mostCorrectThemeElement = document.getElementById("most-correct-theme");
const favoriteCategoriesEmojis = ["🔥", "🥈", "🥉"];
//  Fetch from server the user's statistics, to then display them
fetch("/home/userstatistics")
    .then((result) => __awaiter(this, void 0, void 0, function* () {
    const json = yield result.text();
    const stats = JSON.parse(json);
    updateUi(stats);
    return result;
}))
    .catch((error) => {
    console.error(error);
});
function updateUi(stats) {
    weekCountElement.innerHTML = `You answered <b>${stats.lastWeekRecordsCount}</b> question(s) the last <u>7 days</u>`;
    lifetimeRatioElement.innerHTML = `Lifetime answer correctness : <b>${(stats.ratioCorrectness * 100).toFixed()}% </b>[<span style="color:green">${stats.correctLifetimeCount}</span>/<span style="color: red">${stats.failLifetimeCount}</span>]`;
    if (stats.mostCorrectCategory !== null) {
        mostCorrectThemeElement.innerHTML = `Category with the most correct answers : <u>${stats.mostCorrectCategory.subCategoryName}</u> (<b>${stats.mostCorrectCategory.answerCount}</b> correct answers.)`;
    }
    if (stats.mostAnsweredCategories.length === 0) {
        favoriteThemesListElement.innerHTML =
            "<b>😴 You didn't answer any question for the moment.</b>";
    }
    let favoriteThemeIndex = 0;
    stats.mostAnsweredCategories.forEach(function (cat) {
        const newListItem = document.createElement("li");
        newListItem.classList.add("list-group-item");
        newListItem.classList.add("fav-theme-container");
        const aCategoryName = document.createElement("a");
        aCategoryName.textContent = cat.subCategoryName;
        aCategoryName.href = `/Questions?subcategory=${cat.subCategoryName}`;
        const spanAnswerCount = document.createElement("span");
        spanAnswerCount.textContent =
            favoriteCategoriesEmojis[favoriteThemeIndex] + cat.answerCount;
        spanAnswerCount.style.fontWeight = "bold";
        newListItem.appendChild(aCategoryName);
        newListItem.appendChild(spanAnswerCount);
        favoriteThemesListElement.appendChild(newListItem);
        favoriteThemeIndex++;
    });
}
//# sourceMappingURL=index.js.map