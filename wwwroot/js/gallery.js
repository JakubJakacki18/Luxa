import { getWindowWidth } from './utils.js';
import { loadPhotos, likeOrUnlikePhoto } from './api.js';
import { generateColumns, changeData} from './ui.js';

// Eksponowanie funkcji jako globalnej
window.likeOrUnlikePhoto = likeOrUnlikePhoto;
window.changeData = changeData;

//Zmienne globalne
export var pageNumber = 1;
const pageSize = 30;
const maxQuantityOfColumns = 6;
const pixelsPerColumn = 300;
let isLoadingPhotos = false;
const path = '/assets/';
export const likePath = path + 'hand-thumbs-up-fill.svg';
export const unlikePath = path + 'hand-thumbs-up.svg';
let isNotAllPhotos = true;
export var orderAsc = false;
export var sortBy = "date";
export var tag = "";
export var category = "";


//Wczytanie zdjęc po wczytaniu strony
//window.addEventListener('DOMContentLoaded', async function () {
//	generateColumns(getWindowWidth(), maxQuantityOfColumns, pixelsPerColumn);
//	isLoadingPhotos = true;
//	while (isLoadingPhotos && isNotAllPhotos && (window.innerHeight >= document.body.offsetHeight)) {
//		console.log("pętla wczytywanie danych: " + isLoadingPhotos);
//		console.log('na zew loadPhotos pageNumber: ' + pageNumber+' wykonuje się pętla');
//		if (!(await loadPhotos(pageNumber, pageSize))) {
//			console.log("1.isNotAllPhotos" + isNotAllPhotos);
//			isNotAllPhotos = false;
//			console.log("2.isNotAllPhotos" + isNotAllPhotos);
//			break;
//		}
//		pageNumber++;
//	}
//	isLoadingPhotos = false;
//});

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


window.addEventListener('DOMContentLoaded', async function () {
	tag = document.getElementById('tag')?.value || "";
	await initPhotos();
});

export function setDefaultPageNumberAndIsNotAllPhotos() {
	pageNumber = 1;
	isNotAllPhotos = true;
}

export function setSearchingAttributes(Atributes) {
	orderAsc = Atributes.orderAsc;
	sortBy = Atributes.sortBy;
	tag = Atributes.tag;
	category = Atributes.category;
}

export async function initPhotos() {
	generateColumns(getWindowWidth(), maxQuantityOfColumns, pixelsPerColumn);
	isLoadingPhotos = true;
	while (isLoadingPhotos && isNotAllPhotos && (window.innerHeight >= document.body.offsetHeight)) {
		console.log("pętla: " + isLoadingPhotos);
		if (!(await loadPhotos(pageNumber, pageSize))) {
			isNotAllPhotos = false;
			break;
		}
		pageNumber++;
	}
	isLoadingPhotos = false;

};






