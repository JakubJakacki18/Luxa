import { addPhotosToColumns, changeLikeButton, notifyAboutLackOfPhotos } from './ui.js';

export async function loadPhotos(pageNumber, pageSize) {
	const response = await fetch(`/Home/LoadPhotos?pageNumber=${pageNumber}&pageSize=${pageSize}`);
	if (response.status === 401) {
		alert("Nie jesteś zalogowany. Zaloguj się");
		window.location.href = '/SignIn';
		return false;
	}
	const photos = await response.json();
	if (photos.length > 0) {
		addPhotosToColumns(photos);
		return true;
	}
	else {
		notifyAboutLackOfPhotos();
		return false;
	}
}

export async function likeOrUnlikePhoto(id) {
	try {
		const response = await fetch('/Photos/LikeOrUnlikePhoto/', {
			method: 'POST',
			headers: {
				'Content-Type': 'application/x-www-form-urlencoded'
			},
			body: "idPhoto=" + id
		});
		if (!response.ok) {
			throw new Error(`status: ${response.status}`);
		}
		const result = await response.json();
		changeLikeButton(id);
		console.log(result);
	} catch (error) {
		console.error('błąd: ' + error.message);
	}
}