import {FastifyPluginAsync, FastifyPluginOptions} from 'fastify'
import {config} from '../../config'

import {createApi} from 'unsplash-js'
import {Random} from 'unsplash-js/dist/methods/photos/types'
import {imageToVideo} from './ffmpeg/image-to-video'
import path from 'path'

const api = createApi({
  accessKey: config.unsplash.accessKey,
})

export const register: FastifyPluginAsync<FastifyPluginOptions> = async (instance) => {
  instance.get('/unsplash/random', (req, res) => {
    /* const photo = await api.photos.getRandom({featured: true})
    let random: Random = null

    if (Array.isArray(photo.response)) {
      random = photo.response[0]
    } else {
      random = photo.response
    } */

    // await api.photos.trackDownload({downloadLocation: random.links.download_location})

    const imageTmp = path.join(__dirname, 'test.jpg')
    const {cleanup, read, write} = imageToVideo(imageTmp)

    write.on('finish', async () => {
      await res.status(200).header('Content-Type', 'video/mp4').send(read())
      // await cleanup()
    })
  })

  await Promise.resolve()
}
