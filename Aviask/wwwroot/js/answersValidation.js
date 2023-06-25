const formElement = document.getElementById("questionForm");
const answerElements = Array.from(document.getElementsByClassName("answer"));
const correctAnswerElement = document.getElementById("CorrectAnswer");
correctAnswerElement.addEventListener("input", (e) => {
    const answers = answerElements.map(e => e.value);
    if (!answers.some(v => v == correctAnswerElement.value)) {
        correctAnswerElement.setCustomValidity("tsgfdfsdfsgdfd");
        console.log("trst");
    }
});
//# sourceMappingURL=answersValidation.js.map