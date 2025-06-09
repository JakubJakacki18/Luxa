import { getShortestColumn, getOrder } from './utils.js';
import { unlikePath, likePath, setDefaultPageNumberAndIsNotAllPhotos, setSearchingAttributes, initPhotos } from './gallery.js';

export function generateColumns(windowWidth, maxQuantityOfColumns, pixelsPerColumn) {
	const galery = document.querySelector('.photo-galery');
	var quantity = Math.floor(windowWidth / pixelsPerColumn);
	if (quantity === 0)
		quantity = 1;
	if (quantity > maxQuantityOfColumns)
		quantity = maxQuantityOfColumns;
	for (let i = 0; i < quantity; i++) {
		const columnDiv = document.createElement('div');
		columnDiv.classList.add('column');
		columnDiv.innerHTML = '<div class=columnHeight></div>';
		galery.appendChild(columnDiv)
	}
}

export function changeLikeButton(id) {
	const url = window.location.origin;
	console.log(url + " " + img + " " + like);
	var img = document.getElementById('photo.' + id);
	var like = document.getElementById('likes.' + id);
	let currentValue = parseInt(like.textContent);
	console.log(like);
	var src = img.src;
	console.log('img.src: ' + img.src + ' likePath: ' + likePath)
	if (img.src === url + unlikePath) {
		img.src = url + likePath;
		currentValue++;
	}
	else {
		img.src = url + unlikePath;
		currentValue--;
	}
	like.textContent = currentValue;

}

export function addPhotosToColumns(photos) {
	const columns = document.querySelectorAll('.columnHeight');
	photos.forEach((photo) => {
		const isLiked = photo.isLiked;
		const likeString = isLiked ? likePath : unlikePath;
		const photoDiv = document.createElement('div');
		photoDiv.classList.add('photo', 'trigger');
		const img = new Image();
		img.src = `/Image/${encodeURIComponent(photo.photo.name)}`;
		img.alt = photo.photo.name;

		img.onload = () => {
			photoDiv.innerHTML = `
				<a type="button" href="/Photos/Details/${photo.photo.id}">
					<img src="/Image/${photo.photo.name}" alt="${photo.photo.name}">
				</a>
				<div class="target">
					<div class="like">
						<a onclick="likeOrUnlikePhoto(${photo.photo.id})" type="button">
							<img id="photo.${photo.photo.id}" src="${likeString}">
						</a>
					</div>
					<div class="like-text" id="likes.${photo.photo.id}">${photo.photo.likeCount}</div>
					<div class="user">
						${photo.ownerName}
					</div>
				</div>
			`;

			const shortestColumn = getShortestColumn(columns);
			shortestColumn.appendChild(photoDiv);
			addTrigger(photoDiv); // dodaj animację tylko do tego elementu
		};
	});
}


function addTrigger(element) {
	const target = element.querySelector('.target');
	element.addEventListener('mouseenter', () => {
		target.style.opacity = '1';
	});
	element.addEventListener('mouseleave', () => {
		target.style.opacity = '0';
	});
}

export function notifyAboutLackOfPhotos() {
	const newBox = document.querySelector('.newBox');
	const info = document.createElement('div');
	info.classList.add('photos-info');
	info.id = 'photos-info';
	info.innerHTML = '<div class="alert alert-primary" role="alert">Nie ma więcej zdjęć do wyświetlenia</div>';
	newBox.appendChild(info);
};

export async function changeData() {
	const photoGalery = document.getElementById("photo-galery");
	photoGalery.innerHTML = '';
	setDefaultPageNumberAndIsNotAllPhotos();
	const Atributes = {
		tag: document.getElementById("searchTag")?.value || "",
		category: document.getElementById("category")?.value || "",
		orderAsc: getOrder(document.getElementById("orderDirection")?.value || ""),
		sortBy: document.getElementById("orderBy")?.value || "date"
	};
	setSearchingAttributes(Atributes);
	const info = document.getElementById("photos-info");
	if (info) {
		info.remove();
	}
	await initPhotos();
};