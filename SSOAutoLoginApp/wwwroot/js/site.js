const theme = localStorage.getItem('theme') || 'light';
if (theme === 'dark') {
    document.body.classList.add('dark-mode');
}
document.getElementById('theme-toggle').innerText = theme != 'light' ? "Day" : "Night";
// Function to toggle the theme
function toggleTheme () {
    const currentTheme = document.body.classList.contains('dark-mode') ? 'dark' : 'light';
    const newTheme = currentTheme === 'dark' ? 'light' : 'dark';

    // Toggle dark mode class
    document.body.classList.toggle('dark-mode', newTheme === 'dark');

    document.getElementById('theme-toggle').innerText = newTheme != 'light' ? "Day" : "Night";

    // Save the user's preference to localStorage
    localStorage.setItem('theme', newTheme);
}

// Event listener for the theme toggle button
document.getElementById('theme-toggle').addEventListener('click', toggleTheme);