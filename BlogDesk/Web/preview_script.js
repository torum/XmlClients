

function toggleTheme(theme)
{
    if (theme === 'dark')
    {
        //document.body.classList.remove('light-theme');
        document.body.classList.add('dark-theme');
    }
    else if (theme === 'light')
    {
        document.body.classList.remove('dark-theme');
        //document.body.classList.add('light-theme');
    }
}

