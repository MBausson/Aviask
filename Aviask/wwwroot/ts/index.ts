//  Represents user's themes he answered the most
interface Theme {
    subCategory: number;
    subCategoryName: string;
}

interface MostAnsweredTheme extends Theme {
    answerCount: number;
}

//  Represents user's themes he answered the best
interface FavoriteTheme extends Theme {
    correctAnswerCount: number;
}

interface UserStatistics {
    lastWeekRecordsCount: number;
    correctLifetime: number;
    failLifetime: number;
    ratioCorrectness: number;
    favoriteThemes: Array<MostAnsweredTheme>;
    mostCorrectlyTheme: FavoriteTheme;
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
    lifetimeRatioElement.innerHTML = `Lifetime answer correctness : <b>${
        stats.ratioCorrectness * 100
    }%</b>[<span style="color:green">${
        stats.correctLifetime
    }</span>/<span style="color: red">${stats.failLifetime}</span>]`;

    if (stats.mostCorrectlyTheme !== null) {
        mostCorrectThemeElement.innerHTML = `Category with the most correct answers : <u>${stats.mostCorrectlyTheme.subCategoryName}</u> (<b>${stats.mostCorrectlyTheme.correctAnswerCount}</b> correct answers.)`;
    }

    if (stats.favoriteThemes.length == 0) {
        favoriteThemesListElement.innerHTML =
            "<b>😴 You didn't answer any question for the moment.</b>";
    }

    let favoriteThemeIndex = 0;

    stats.favoriteThemes.forEach(function (theme: MostAnsweredTheme): void {
        const newListItem: HTMLLIElement = document.createElement("li");
        newListItem.classList.add("list-group-item");
        newListItem.classList.add("fav-theme-container");

        const aCategoryName: HTMLAnchorElement = document.createElement("a");
        aCategoryName.textContent = theme.subCategoryName;
        aCategoryName.href = `/Questions?subcategory=${theme.subCategoryName}`;

        const spanAnswerCount: HTMLSpanElement = document.createElement("span");
        spanAnswerCount.textContent =
            favoriteCategoriesEmojis[favoriteThemeIndex] + theme.answerCount;
        spanAnswerCount.style.fontWeight = "bold";

        newListItem.appendChild(aCategoryName);
        newListItem.appendChild(spanAnswerCount);

        favoriteThemesListElement.appendChild(newListItem);
        favoriteThemeIndex++;
    });
}
