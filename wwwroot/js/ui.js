import { getShortestColumn } from './utils.js';
import { unlikePath, likePath } from './gallery.js';

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
		console.log('niby prawda że unlike jest');
		currentValue++;
	}
	else {
		img.src = url + unlikePath;
		console.log('niby prawda że like jest');
		currentValue--;
	}
	like.textContent = currentValue;

}

export function addPhotosToColumns(photos) {
	const columns = document.querySelectorAll('.columnHeight');
	photos.forEach((photo) => {
		console.log(photo);
		console.log("len:" + columns.length);
		console.log("/Image/" + photo.photo.name);
		const column = getShortestColumn(columns);
		const photoDiv = document.createElement('div');
		//trzeba pobrać wartość z modelu
		const isLiked = photo.isLiked
		if (isLiked) {
			var likeString = likePath;
		}
		else {
			var likeString = unlikePath;
		}
		photoDiv.classList.add('photo', 'trigger');
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
		column.appendChild(photoDiv);
	});
	addTrigger();
}

function addTrigger() {
	const triggers = document.querySelectorAll('.photo.trigger');
	console.log("test");
	triggers.forEach(trigger => {
		const target = trigger.querySelector('.target');

		trigger.addEventListener('mouseenter', function () {
			target.style.opacity = '1';
			console.log("zmiana na 1");
		});

		trigger.addEventListener('mouseleave', function () {
			target.style.opacity = '0';
			console.log("zmiana na 0");
		});
	});
};

export function notifyAboutLackOfPhotos()
{
	const newBox = document.querySelector('.newBox');
	const info = document.createElement('div');
	info.classList.add('photos-info');
	info.id = 'photos-info';
	info.innerHTML = '<div class="alert alert-primary" role="alert">Nie ma więcej zdjęć do wyświetlenia</div>';
	newBox.appendChild(info);
};
