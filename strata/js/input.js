// Input handling - tracks keyboard state
const keys = {};
const justPressed = {};

export function initInput() {
    window.addEventListener('keydown', (e) => {
        if (!keys[e.code]) {
            justPressed[e.code] = true;
        }
        keys[e.code] = true;
        // Prevent scrolling with arrow keys/space
        if (['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'Space'].includes(e.code)) {
            e.preventDefault();
        }
    });

    window.addEventListener('keyup', (e) => {
        keys[e.code] = false;
    });
}

export function isKeyDown(code) {
    return !!keys[code];
}

export function isKeyJustPressed(code) {
    return !!justPressed[code];
}

export function clearJustPressed() {
    for (const key in justPressed) {
        delete justPressed[key];
    }
}

// Mouse state
const mouse = { x: 0, y: 0, clicked: false, justClicked: false };

export function initMouse(canvas) {
    canvas.addEventListener('mousemove', (e) => {
        const rect = canvas.getBoundingClientRect();
        mouse.x = e.clientX - rect.left;
        mouse.y = e.clientY - rect.top;
    });

    canvas.addEventListener('mousedown', (e) => {
        mouse.clicked = true;
        mouse.justClicked = true;
    });

    canvas.addEventListener('mouseup', () => {
        mouse.clicked = false;
    });
}

export function getMouse() {
    return { ...mouse };
}

export function clearMouseJustClicked() {
    mouse.justClicked = false;
}
