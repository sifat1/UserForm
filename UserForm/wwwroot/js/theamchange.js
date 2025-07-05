const toggle = document.getElementById("themeToggle");
const themeLink = document.getElementById("bootstrap-theme");
const savedTheme = localStorage.getItem("theme") || "flatly";
themeLink.href = `https://cdn.jsdelivr.net/npm/bootswatch@5.3.3/dist/${savedTheme}/bootstrap.min.css`;
toggle.checked = savedTheme === "solar";
toggle.addEventListener("change", () => {
    const selectedTheme = toggle.checked ? "solar" : "flatly";
    themeLink.href = `https://cdn.jsdelivr.net/npm/bootswatch@5.3.3/dist/${selectedTheme}/bootstrap.min.css`;
    localStorage.setItem("theme", selectedTheme);
});