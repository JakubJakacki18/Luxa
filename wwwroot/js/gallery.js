import { getWindowWidth } from './utils.js';
import { loadPhotos, likeOrUnlikePhoto } from './api.js';
import { generateColumns } from './ui.js';

// Eksponowanie funkcji jako globalnej
window.likeOrUnlikePhoto = likeOrUnlikePhoto; 

//Zmienne globalne
let pageNumber = 1;
const pageSize = 30;
const maxQuantityOfColumns = 6;
const pixelsPerColumn = 300;
let isLoadingPhotos = false;
const path = '/assets/';
export const likePath = path + 'hand-thumbs-up-fill.svg';
export const unlikePath = path + 'hand-thumbs-up.svg';
let isNotAllPhotos = true;

//Wczytanie zdjęc po wczytaniu strony
window.addEventListener('DOMContentLoaded', async function () {
	generateColumns(getWindowWidth(), maxQuantityOfColumns, pixelsPerColumn);
	isLoadingPhotos = true;
	while (isLoadingPhotos && isNotAllPhotos && (window.innerHeight >= document.body.offsetHeight)) {
		console.log("pętla wczytywanie danych: " + isLoadingPhotos);
		console.log('na zew loadPhotos pageNumber: ' + pageNumber+' wykonuje się pętla');
		if (!(await loadPhotos(pageNumber, pageSize))) {
			console.log("1.isNotAllPhotos" + isNotAllPhotos);
			isNotAllPhotos = false;
			console.log("2.isNotAllPhotos" + isNotAllPhotos);
			break;
		}
		pageNumber++;
	}
	isLoadingPhotos = false;
});

//Wczytanie zdjęć po scrollu
window.addEventListener('scroll', () => {
	if (!isLoadingPhotos && isNotAllPhotos && (window.innerHeight + window.scrollY) >= document.body.offsetHeight) {
		console.log("Scroll zadziałał, wykonuje się if");
		isLoadingPhotos = true;
		loadPhotos().then((result) => {
			isNotAllPhotos = result;
			isLoadingPhotos = false;
		});
	}
});


