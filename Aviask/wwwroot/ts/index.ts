interface StatisticsCategory {
    subCategory: number;
    subCategoryName: string;
    answerCount: number;
}

interface UserStatistics {
    lastWeekRecordsCount: number;
    correctLifetimeCount: number;
    failLifetimeCount: number;
    ratioCorrectness: number;
    mostAnsweredCategories: Array<StatisticsCategory>;
    mostCorrectCategory: StatisticsCategory;
}

const weekCountElement: HTMLElement = document.getElementById("week-count");
const lifetimeRatioElement: HTMLElement =
    document.getElementById("lifetime-ratio");
const favoriteThemesListElement: HTMLElement = document.getElementById(
    "list-favourite-themes"
);
const mostCorrectThemeElement: HTMLElement =
    document.getElementById("most-correct-theme");

const favoriteCategoriesEmojis: Array<string> = ["🔥", "🥈", "🥉"];

//  Fetch from server the user's statistics, to then display them
fetch("/home/userstatistics")
    .then(async (result) => {
        const json: string = await result.text();
        const stats: UserStatistics = JSON.parse(json);

        updateUi(stats);

        return result;
    })
    .catch((error) => {
        console.error(error);
    });

function updateUi(stats: UserStatistics): void {
    weekCountElement.innerHTML = `You answered <b>${stats.lastWeekRecordsCount}</b> question(s) the last <u>7 days</u>`;
    lifetimeRatioElement.innerHTML = `Lifetime answer correctness : <b>${(stats.ratioCorrectness * 100).toFixed()}% </b>[<span style="color:green">${
        stats.correctLifetimeCount
    }</span>/<span style="color: red">${stats.failLifetimeCount}</span>]`;

    if (stats.mostCorrectCategory !== null) {
        mostCorrectThemeElement.innerHTML = `Category with the most correct answers : <u>${stats.mostCorrectCategory.subCategoryName}</u> (<b>${stats.mostCorrectCategory.answerCount}</b> correct answers.)`;
    }

    if (stats.mostAnsweredCategories.length === 0) {
        favoriteThemesListElement.innerHTML =
            "<b>😴 You didn't answer any question for the moment.</b>";
    }

    let favoriteThemeIndex = 0;

    stats.mostAnsweredCategories.forEach(function (cat: StatisticsCategory): void {
        const newListItem: HTMLLIElement = document.createElement("li");
        newListItem.classList.add("list-group-item", "flex", "justify-between");

        const aCategoryName: HTMLAnchorElement = document.createElement("a");
        aCategoryName.classList.add("underline");
        aCategoryName.textContent = cat.subCategoryName;
        aCategoryName.href = `/Questions?subcategory=${cat.subCategoryName}`;

        const spanAnswerCount: HTMLSpanElement = document.createElement("span");
        spanAnswerCount.textContent =
            favoriteCategoriesEmojis[favoriteThemeIndex] + cat.answerCount;
        spanAnswerCount.style.fontWeight = "bold";

        newListItem.appendChild(aCategoryName);
        newListItem.appendChild(spanAnswerCount);

        favoriteThemesListElement.appendChild(newListItem);
        favoriteThemeIndex++;
    });
}
