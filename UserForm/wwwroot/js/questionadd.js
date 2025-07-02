// Shared variables
let questionIndex = 0;
let tags = [];

// Question template creation
function createQuestionBlock(index) {
    return `
        <div class="question-block mb-4 p-3 border rounded" data-index="${index}">
            <div class="form-check mb-2">
                <input type="checkbox" class="form-check-input delete-checkbox" />
                <label class="form-check-label">Select to Delete</label>
            </div>
            <div class="mb-2">
                <label class="form-label">Question Text</label>
                <input type="text" name="Questions[${index}].QuestionText" class="form-control" required />
            </div>
            <div class="mb-2">
                <label class="form-label">Question Type</label>
                <select class="form-select question-type" name="Questions[${index}].QuestionType" data-index="${index}">
                    <option value="Text">Text</option>
                    <option value="Number">Number</option>
                    <option value="MultipleChoice">Multiple Choice</option>
                </select>
            </div>
            <div class="question-options" id="options-${index}"></div>
        </div>`;
}

// Question management
function addQuestion() {
    const container = document.getElementById("questions-container");
    if (!container) return;
    container.insertAdjacentHTML("beforeend", createQuestionBlock(questionIndex));
    questionIndex++;
}

// Option management
function addOptionField(questionIdx) {
    const optionsDiv = document.getElementById(`option-list-${questionIdx}`);
    if (!optionsDiv) return;
    const optionCount = optionsDiv.querySelectorAll("input[type='text']").length;
    optionsDiv.insertAdjacentHTML("beforeend", `
        <div class="input-group mb-2 option-item">
            <div class="input-group-text">
                <input type="checkbox" class="form-check-input option-delete-checkbox" />
            </div>
            <span class="input-group-text">Option ${optionCount + 1}</span>
            <input type="text" class="form-control" name="Questions[${questionIdx}].Options[${optionCount}]" required />
        </div>
    `);
}

function deleteSelectedOptions(questionIdx) {
    const optionsDiv = document.getElementById(`option-list-${questionIdx}`);
    if (!optionsDiv) return;
    optionsDiv.querySelectorAll(".option-delete-checkbox:checked").forEach(cb =>
        cb.closest(".option-item").remove()
    );
}

// Tag management
function updateTagUI() {
    const tagContainer = document.getElementById("tag-container");
    const tagInput = document.getElementById("tag-input");
    const hiddenInput = document.getElementById("Tags");

    if (!tagContainer || !tagInput || !hiddenInput) return;

    tagContainer.innerHTML = '';
    tags.forEach((tag, index) => {
        const tagEl = document.createElement("span");
        tagEl.className = "badge bg-primary me-1 mb-1";
        tagEl.innerHTML = `${tag} <button type="button" class="btn-close btn-close-white btn-sm ms-1" onclick="formBuilder.removeTag(${index})"></button>`;
        tagContainer.appendChild(tagEl);
    });
    tagContainer.appendChild(tagInput);
    hiddenInput.value = tags.join(",");
}

function removeTag(index) {
    tags.splice(index, 1);
    updateTagUI();
}

// Event handlers
function setupQuestionTypeChangeHandler() {
    document.addEventListener("change", function (e) {
        if (e.target.classList.contains("question-type")) {
            const index = e.target.getAttribute("data-index");
            const type = e.target.value;
            const container = document.getElementById(`options-${index}`);
            if (!container) return;
            container.innerHTML = "";

            if (type === "Text") {
                container.innerHTML = `<label>Answer (Multiline)</label><textarea class="form-control" rows="3" disabled></textarea>`;
            } else if (type === "Number") {
                container.innerHTML = `<label>Answer (Numeric)</label><input type="number" class="form-control" disabled />`;
            } else if (type === "MultipleChoice") {
                container.innerHTML = `
                    <label>Options</label>
                    <div id="option-list-${index}"></div>
                    <button type="button" class="btn btn-sm btn-outline-primary mt-2 me-2" onclick="formBuilder.addOptionField(${index})">Add Option</button>
                    <button type="button" class="btn btn-sm btn-outline-danger mt-2" onclick="formBuilder.deleteSelectedOptions(${index})">Delete Selected Options</button>
                `;
                addOptionField(index);
            }
        }
    });
}

// Initialize form with existing data
function initializeForm(questions, initialTags) {
    questions.forEach((q, i) => {
        const container = document.getElementById("questions-container");
        container.insertAdjacentHTML("beforeend", createQuestionBlock(questionIndex));
        const block = container.lastElementChild;
        block.querySelector(`input[name="Questions[${questionIndex}].QuestionText"]`).value = q.QuestionText;
        block.querySelector(`select[name="Questions[${questionIndex}].QuestionType"]`).value = q.QuestionType;
        const event = new Event("change", { bubbles: true });
        block.querySelector(`select[name="Questions[${questionIndex}].QuestionType"]`).dispatchEvent(event);
        if (q.QuestionType === "MultipleChoice" && q.Options) {
            const optionList = document.getElementById(`option-list-${questionIndex}`);
            q.Options.forEach((opt, j) => {
                addOptionField(questionIndex);
                const input = optionList.querySelector(`input[name="Questions[${questionIndex}].Options[${j}]"]`);
                if (input) input.value = opt;
            });
        }
        questionIndex++;
    });

    tags = initialTags;
    updateTagUI();
}
window.formBuilder = {
    questionIndex,
    tags,
    createQuestionBlock,
    addQuestion,
    addOptionField,
    deleteSelectedOptions,
    updateTagUI,
    removeTag,
    setupQuestionTypeChangeHandler,
    initializeForm
};