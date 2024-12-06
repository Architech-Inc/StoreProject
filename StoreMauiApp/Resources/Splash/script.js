const at = document.querySelector('#at');
const loaderElement = document.querySelector('.loading');
const dotsEl = document.querySelector('#dots');
const addAnim = () => {
	at.classList.add('start-anim')
}
const removeAnim = () => {
	at.classList.remove('start-anim')
}
const showLoader = () => {
	loaderElement.classList.add('l-fadeIn');
	loaderElement.style.display = 'flex';
}
const hideLoader = () => {
	loaderElement.classList.add('l-fadeOut')
	setTimeout(() => {
		removeFades();
	}, 200);
}
const removeFades = () => {
	loaderElement.classList.remove('l-fadeIn');
	loaderElement.classList.remove('l-fadeOut');
	loaderElement.style.display = 'none';
	removeAnim();
}
const Dots = (i, text) => {
	if (i != 0) {
		text = text + '.';
		i--;
	}
	if (i == 0) {
		text = '';
		i = 4;
	}
	setTimeout(() => {
		Dots(i, text);
	}, 350)
	dots.innerText = text;
}
function atStart() {
	Dots(4, "");
	showLoader();
	addAnim();
}
function atClose(t) {
	setTimeout(() => {
		hideLoader();
	}, t);
}
atStart();